using Google.Cloud.Firestore;
using IptasPeyzajApi.Backend.BakimPlanlari.Models;
using IptasPeyzajApi.Backend.Musteriler.Helpers;
using IptasPeyzajApi.Backend.Musteriler.Models;
using SkiaSharp;

namespace IptasPeyzajApi.Backend.BakimPlanlari.Helpers;

public class BakimPlaniHelper
{
   
    private readonly FirestoreDb _db;
    private readonly GoogleDriveStorage _driveStorage;
    private const string CollectionName = "bakimPlanlari";

    public BakimPlaniHelper(
        FirestoreDb db,
        GoogleDriveStorage driveStorage)
    {
        _db = db;
        _driveStorage = driveStorage;
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

        string oncesiDriveDosyaId = await ResmiDriveaKaydet(
            id,
            "oncesi",
            oncesiResim);

        string sonrasiDriveDosyaId = await ResmiDriveaKaydet(
            id,
            "sonrasi",
            sonrasiResim);

        // 🔥 2. BAKIM GÜNCELLE
        var updateData = new Dictionary<string, object>
    {
        { "DurumKodu", "T" },
        { "IslemNotu", islemNotu ?? "" },
        { "IslemTarihi", DateTime.UtcNow }
    };

        await docRef.UpdateAsync(updateData);

        // 🔥 3. DETAY KOLEKSİYONA YAZ
        await BakimDetayEkle(
            id,
            personelIdleri,
            oncesiDriveDosyaId,
            sonrasiDriveDosyaId);

        return await BakimGetir(id);
    }

    private async Task<string> ResmiDriveaKaydet(
        string bakimId,
        string resimTipi,
        IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return string.Empty;

        const long maksimumDosyaBoyutu = 15 * 1024 * 1024;

        if (file.Length > maksimumDosyaBoyutu)
            throw new Exception("Resim en fazla 15 MB olabilir.");

        if (string.IsNullOrWhiteSpace(file.ContentType) ||
            !file.ContentType.StartsWith(
                "image/",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new Exception("Yalnızca resim dosyası yüklenebilir.");
        }

        using var inputStream = file.OpenReadStream();
        using var original = SKBitmap.Decode(inputStream);

        if (original == null)
            throw new Exception("Resim okunamadı.");

        int yeniGenislik = Math.Min(original.Width, 1000);
        int yeniYukseklik = Math.Max(
            1,
            (int)Math.Round(
                (double)original.Height /
                original.Width *
                yeniGenislik));

        using SKBitmap? resized =
            yeniGenislik == original.Width
                ? original.Copy()
                : original.Resize(
                    new SKImageInfo(
                        yeniGenislik,
                        yeniYukseklik),
                    SKSamplingOptions.Default);

        if (resized == null)
            throw new Exception("Resim hazırlanamadı.");

        using var image = SKImage.FromBitmap(resized);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 65);

        if (data == null)
            throw new Exception("Resim JPG biçimine dönüştürülemedi.");

        using var outputStream = new MemoryStream();
        data.SaveTo(outputStream);
        outputStream.Position = 0;

        string dosyaAdi =
            $"{bakimId}-{resimTipi}-" +
            $"{DateTime.UtcNow:yyyyMMddHHmmss}-" +
            $"{Guid.NewGuid():N}.jpg";

        return await _driveStorage.JpegYukleAsync(
            outputStream,
            dosyaAdi);
    }
    public async Task BakimDetayEkle(
     string bakimId,
     List<int> personelNolari,
     string? oncesiDriveDosyaId,
     string? sonrasiDriveDosyaId)
    {
        foreach (var p in personelNolari)
        {
            // PERSONEL KAYDI HER ZAMAN OLUŞSUN
            if (string.IsNullOrEmpty(oncesiDriveDosyaId) &&
                string.IsNullOrEmpty(sonrasiDriveDosyaId))
            {
                await _db.Collection("bakimDetaylari").AddAsync(new BakimDetay
                {
                    BakimId = bakimId,
                    PersonelNo = p,
                    ResimTip = "",
                    ResimUrl = "",
                    DriveDosyaId = ""
                });

                continue;
            }

            // ÖNCESİ
            if (!string.IsNullOrEmpty(oncesiDriveDosyaId))
            {
                await _db.Collection("bakimDetaylari").AddAsync(new BakimDetay
                {
                    BakimId = bakimId,
                    PersonelNo = p,
                    ResimTip = "O",
                    ResimUrl = "",
                    DriveDosyaId = oncesiDriveDosyaId
                });
            }

            // SONRASI
            if (!string.IsNullOrEmpty(sonrasiDriveDosyaId))
            {
                await _db.Collection("bakimDetaylari").AddAsync(new BakimDetay
                {
                    BakimId = bakimId,
                    PersonelNo = p,
                    ResimTip = "S",
                    ResimUrl = "",
                    DriveDosyaId = sonrasiDriveDosyaId
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
                ResimAdresiniHazirla(detay);
                liste.Add(detay);
            }
        }

        return liste;
    }

    public async Task<BakimDetay?> BakimDetayGetir(
        string detayId)
    {
        DocumentSnapshot doc = await _db
            .Collection("bakimDetaylari")
            .Document(detayId)
            .GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        BakimDetay detay = doc.ConvertTo<BakimDetay>();
        detay.Id = doc.Id;
        ResimAdresiniHazirla(detay);

        return detay;
    }

    private static void ResimAdresiniHazirla(
        BakimDetay detay)
    {
        if (!string.IsNullOrWhiteSpace(detay.DriveDosyaId) &&
            !string.IsNullOrWhiteSpace(detay.Id))
        {
            detay.ResimUrl =
                $"/api/BakimPlanlari/detaylar/{detay.Id}/resim";
        }
    }
    public async Task<(int Guncellenen, int Eslesmeyen)>
    EksikMusteriIdleriniDoldur()
    {
        QuerySnapshot musteriSnapshot =
            await _db.Collection("musteriler")
                .GetSnapshotAsync();

        Dictionary<int, string> musteriIdMap = new();

        foreach (DocumentSnapshot doc
            in musteriSnapshot.Documents)
        {
            if (!doc.Exists)
                continue;

            Musteri musteri =
                doc.ConvertTo<Musteri>();

            musteriIdMap[musteri.MusteriNo] =
                doc.Id;
        }

        QuerySnapshot bakimSnapshot =
            await _db.Collection(CollectionName)
                .GetSnapshotAsync();

        int guncellenen = 0;
        int eslesmeyen = 0;

        foreach (DocumentSnapshot doc
            in bakimSnapshot.Documents)
        {
            if (!doc.Exists)
                continue;

            BakimPlani bakim =
                doc.ConvertTo<BakimPlani>();

            // ID zaten varsa dokunma
            if (!string.IsNullOrWhiteSpace(
                bakim.MusteriId))
            {
                continue;
            }

            if (
                musteriIdMap.TryGetValue(
                    bakim.MusteriNo,
                    out string? musteriId
                )
            )
            {
                await doc.Reference.UpdateAsync(
                    new Dictionary<string, object>
                    {
                    { "MusteriId", musteriId }
                    }
                );

                guncellenen++;
            }
            else
            {
                eslesmeyen++;
            }
        }

        return (guncellenen, eslesmeyen);
    }

}
