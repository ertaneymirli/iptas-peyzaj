using Google.Cloud.Firestore;
using IptasPeyzajApi.Backend.BakimPlanlari.Models;
using IptasPeyzajApi.Backend.Musteriler.Helpers;
using IptasPeyzajApi.Backend.Musteriler.Models;

namespace IptasPeyzajApi.Backend.BakimPlanlari.Helpers;

public class BakimPlaniHelper
{
   
    private readonly FirestoreDb _db;
    private const string CollectionName = "bakimPlanlari";

    public BakimPlaniHelper(FirestoreDb db)
    {
        _db = db;
    }

    public async Task<List<BakimPlani>> TumBakimlariGetir()
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .OrderBy("BakimTarihi")
            .GetSnapshotAsync();

        List<BakimPlani> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                BakimPlani item = doc.ConvertTo<BakimPlani>();
                item.Id = doc.Id;
                liste.Add(item);
            }
        }
       
        return liste;
    }

    public async Task<List<BakimPlani>> DurumaGoreGetir(string durumKodu)
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .WhereEqualTo("DurumKodu", durumKodu)
            .OrderBy("BakimTarihi")
            .GetSnapshotAsync();

        return SnapshotToList(snapshot);
    }

    public async Task<BakimPlani?> BakimGetir(string id)
    {
        DocumentSnapshot doc = await _db.Collection(CollectionName)
            .Document(id)
            .GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        BakimPlani item = doc.ConvertTo<BakimPlani>();
        item.Id = doc.Id;

        return item;
    }

    public async Task<BakimPlani> BakimEkle(BakimPlani bakim)
    {
        bakim.KayitTarihi = DateTime.UtcNow;
        bakim.BakimTarihi = UtcYap(bakim.BakimTarihi);

        DocumentReference addedDoc = await _db.Collection(CollectionName).AddAsync(bakim);
        bakim.Id = addedDoc.Id;

        return bakim;
    }

    public async Task<BakimPlani?> DurumGuncelle(string id, string durumKodu, string islemNotu)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "DurumKodu", durumKodu },
            { "IslemTarihi", DateTime.UtcNow },
            { "IslemNotu", islemNotu ?? string.Empty }
        });

        return await BakimGetir(id);
    }

    public async Task<BakimPlani?> Ertele(string id, DateTime yeniTarih, string islemNotu)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "DurumKodu", "E" },
            { "BakimTarihi", UtcYap(yeniTarih) },
            { "IslemTarihi", DateTime.UtcNow },
            { "IslemNotu", islemNotu ?? "Bakım ertelendi." }
        });

        return await BakimGetir(id);
    }

    private static List<BakimPlani> SnapshotToList(QuerySnapshot snapshot)
    {
        List<BakimPlani> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                BakimPlani item = doc.ConvertTo<BakimPlani>();
                item.Id = doc.Id;
                liste.Add(item);
            }
        }

        return liste;
    }

    private static DateTime UtcYap(DateTime tarih)
    {
        if (tarih.Kind == DateTimeKind.Utc)
            return tarih;

        return DateTime.SpecifyKind(tarih, DateTimeKind.Utc);
    }
    public async Task MusteriIcinBakimPlanlariOlustur(Musteri musteri)
    {
        if (musteri.BakimTarihleri == null || musteri.BakimTarihleri.Count == 0)
            return;

        foreach (DateTime tarih in musteri.BakimTarihleri)
        {
            BakimPlani bakim = new()
            {
                MusteriId = musteri.Id ?? string.Empty,
                MusteriNo = musteri.MusteriNo,
                AdSoyad = $"{musteri.Ad} {musteri.Soyad}",
                Telefon = musteri.Telefon,
                BakimTarihi = UtcYap(tarih),
                DurumKodu = "B",
                Aciklama = "Müşteri kaydından otomatik oluşturuldu.",
                KayitTarihi = DateTime.UtcNow
            };

            await _db.Collection(CollectionName).AddAsync(bakim);
        }
    }
    public async Task MusteriBakimPlaniniGuncelle(Musteri musteri, string neden)
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .WhereEqualTo("MusteriNo", musteri.MusteriNo)
            .WhereEqualTo("DurumKodu", "B")
            .GetSnapshotAsync();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            await doc.Reference.UpdateAsync(new Dictionary<string, object>
        {
            { "DurumKodu", "I" },
            { "IslemTarihi", DateTime.UtcNow },
            { "IslemNotu", neden },
            { "Aciklama", neden }
        });
        }

        await MusteriIcinBakimPlanlariOlustur(musteri);
    }
    public async Task MusteriyeAitBakimlariPasifYap(int musteriNo)
    {
        QuerySnapshot snapshot = await _db.Collection("bakimPlanlari")
            .WhereEqualTo("MusteriNo", musteriNo)
            .WhereEqualTo("DurumKodu", "B") // sadece bekleyenler
            .GetSnapshotAsync();

        foreach (var doc in snapshot.Documents)
        {
            await doc.Reference.UpdateAsync(new Dictionary<string, object>
        {
            { "DurumKodu", "I" },
            { "IslemTarihi", DateTime.UtcNow },
            { "IslemNotu", "Müşteri pasif edildiği için bakım iptal edildi." },
            { "Aciklama", "Müşteri pasif edildi." }
        });
        }
    }
    public async Task<BakimPlani?> BakimTamamla(
     string id,
     List<int> personelIdleri,
     string islemNotu,
     IFormFile? oncesiResim,
     IFormFile? sonrasiResim)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        // 🔥 1. RESİMLERİ KAYDET
        string oncesiUrl = await ResimKaydet(oncesiResim);
        string sonrasiUrl = await ResimKaydet(sonrasiResim);

        // 🔥 2. BAKIM GÜNCELLE
        var updateData = new Dictionary<string, object>
    {
        { "DurumKodu", "T" },
        { "IslemNotu", islemNotu ?? "" },
        { "IslemTarihi", DateTime.UtcNow }
    };

        await docRef.UpdateAsync(updateData);

        // 🔥 3. DETAY KOLEKSİYONA YAZ
        await BakimDetayEkle(id, personelIdleri, oncesiUrl, sonrasiUrl);

        return await BakimGetir(id);
    }

    private async Task<string> ResimKaydet(IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return string.Empty;

        string extension = Path.GetExtension(file.FileName).ToLower();

        string[] izinliUzantilar = [".jpg", ".jpeg", ".png", ".webp"];

        if (!izinliUzantilar.Contains(extension))
            throw new Exception("Sadece jpg, jpeg, png veya webp yüklenebilir.");

        string uploadsPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "uploads",
            "bakimlar"
        );

        if (!Directory.Exists(uploadsPath))
            Directory.CreateDirectory(uploadsPath);

        string fileName = $"{Guid.NewGuid()}{extension}";
        string fullPath = Path.Combine(uploadsPath, fileName);

        using FileStream stream = new(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return $"/uploads/bakimlar/{fileName}";
    }
    public async Task BakimDetayEkle(
     string bakimId,
     List<int> personelNolari,
     string? oncesiUrl,
     string? sonrasiUrl)
    {
        foreach (var p in personelNolari)
        {
            // PERSONEL KAYDI HER ZAMAN OLUŞSUN
            if (string.IsNullOrEmpty(oncesiUrl) &&
                string.IsNullOrEmpty(sonrasiUrl))
            {
                await _db.Collection("bakimDetaylari").AddAsync(new BakimDetay
                {
                    BakimId = bakimId,
                    PersonelNo = p,
                    ResimTip = "",
                    ResimUrl = ""
                });

                continue;
            }

            // ÖNCESİ
            if (!string.IsNullOrEmpty(oncesiUrl))
            {
                await _db.Collection("bakimDetaylari").AddAsync(new BakimDetay
                {
                    BakimId = bakimId,
                    PersonelNo = p,
                    ResimTip = "O",
                    ResimUrl = oncesiUrl
                });
            }

            // SONRASI
            if (!string.IsNullOrEmpty(sonrasiUrl))
            {
                await _db.Collection("bakimDetaylari").AddAsync(new BakimDetay
                {
                    BakimId = bakimId,
                    PersonelNo = p,
                    ResimTip = "S",
                    ResimUrl = sonrasiUrl
                });
            }
        }
    }
    public async Task<List<BakimDetay>> BakimDetaylariGetir(string bakimId)
    {
        QuerySnapshot snapshot = await _db.Collection("bakimDetaylari")
            .WhereEqualTo("BakimId", bakimId)
            .GetSnapshotAsync();

        List<BakimDetay> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                BakimDetay detay = doc.ConvertTo<BakimDetay>();
                detay.Id = doc.Id;
                liste.Add(detay);
            }
        }

        return liste;
    }

}
