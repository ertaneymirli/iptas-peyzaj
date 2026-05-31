using Google.Cloud.Firestore;
using IptasPeyzajApi.Backend.Models;
using IptasPeyzajApi.Backend.Musteriler.Models;
using IptasPeyzajApi.Backend.BakimPlanlari.Helpers;


namespace IptasPeyzajApi.Backend.Musteriler.Helpers;

public class MusteriHelper
{
    private readonly FirestoreDb _db;
    private readonly BakimPlaniHelper _bakimPlaniHelper;
    private const string CollectionName = "musteriler";

    public MusteriHelper(FirestoreDb db, BakimPlaniHelper bakimPlaniHelper)
    {
        _db = db;
        _bakimPlaniHelper = bakimPlaniHelper;
    }

    public async Task<List<Musteri>> TumMusterileriGetir()
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .OrderByDescending("KayitTarihi")
            .GetSnapshotAsync();

        List<Musteri> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                Musteri musteri = doc.ConvertTo<Musteri>();
                musteri.Id = doc.Id;
                liste.Add(musteri);
            }
        }
        liste = liste.Where(x => x.DurumKodu != "P").ToList();
        foreach (var m in liste)
        {
            var tarih = await SonrakiBakimTarihiGetir(m.MusteriNo);
            m.BakimTarihi = tarih ?? default;
        }
        return liste;
     
    }

    public async Task<Musteri?> MusteriGetir(string id)
    {
        DocumentSnapshot doc = await _db.Collection(CollectionName)
            .Document(id)
            .GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        Musteri musteri = doc.ConvertTo<Musteri>();
        musteri.Id = doc.Id;

        return musteri;
    }
    public async Task<Musteri?> MusteriNodanGetir(int musteriNo)
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .WhereEqualTo("MusteriNo", musteriNo)
            .Limit(1)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
            return null;

        DocumentSnapshot doc = snapshot.Documents[0];

        Musteri musteri = doc.ConvertTo<Musteri>();
        musteri.Id = doc.Id;

        return musteri;
    }
    private void TcKontrol(string tc)
    {
        if (!string.IsNullOrEmpty(tc))
        {
            if (tc.Length != 11 || !tc.All(char.IsDigit))
            {
                throw new Exception("TC Kimlik No 11 haneli ve sadece rakam olmalıdır.");
            }
        }
    }
    public async Task<Musteri> MusteriEkle(Musteri musteri)
    {
        musteri.KayitTarihi = DateTime.UtcNow;
        musteri.MusteriNo = await YeniMusteriNoOlustur();

        TarihleriUtcYap(musteri);

        musteri.BakimTarihleri = BakimTarihleriOlustur(
            musteri.BakimTarihi,
            musteri.BitisTarihi,
            musteri.PeriyodikBakim,
            musteri.PeriyodikBakimTuru,
            musteri.BelirliGunler
        );

        DocumentReference addedDoc = await _db.Collection(CollectionName).AddAsync(musteri);
        musteri.Id = addedDoc.Id;

        await _bakimPlaniHelper.MusteriIcinBakimPlanlariOlustur(musteri);

        return musteri;
    }

    public async Task<Musteri?> MusteriGuncelle(string id, Musteri musteri)
    {
        TcKontrol(musteri.Tc);
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        var eskiMusteri = doc.ConvertTo<Musteri>();

        musteri.MusteriNo = eskiMusteri.MusteriNo;
        musteri.KayitTarihi = eskiMusteri.KayitTarihi;

        TarihleriUtcYap(musteri);

        musteri.BakimTarihleri = BakimTarihleriOlustur(
            musteri.BakimTarihi,
            musteri.BitisTarihi,
            musteri.PeriyodikBakim,
            musteri.PeriyodikBakimTuru,
            musteri.BelirliGunler
        );
        bool bakimPlaniDegisti =
    eskiMusteri.BakimTarihi != musteri.BakimTarihi ||
    eskiMusteri.BitisTarihi != musteri.BitisTarihi ||
    eskiMusteri.PeriyodikBakim != musteri.PeriyodikBakim ||
    eskiMusteri.PeriyodikBakimTuru != musteri.PeriyodikBakimTuru;
        await docRef.SetAsync(musteri, SetOptions.Overwrite);
        if (bakimPlaniDegisti)
        {
            await _bakimPlaniHelper.MusteriBakimPlaniniGuncelle(
                musteri,
                "Müşteri bakım tarihi veya periyodu değiştiği için eski bekleyen bakım planı iptal edildi."
            );
        }
        musteri.Id = id;
        return musteri;
    }

    public async Task<bool> MusteriSil(string id)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return false;

        await docRef.DeleteAsync();
        return true;
    }

    private async Task<int> YeniMusteriNoOlustur()
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .OrderByDescending("MusteriNo")
            .Limit(1)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
            return 1;

        int sonMusteriNo = snapshot.Documents[0].GetValue<int>("MusteriNo");
        return sonMusteriNo + 1;
    }

    private static void TarihleriUtcYap(Musteri musteri)
    {
        musteri.DogumTarihi = UtcYap(musteri.DogumTarihi);
        musteri.SozlesmeTarihi = UtcYap(musteri.SozlesmeTarihi);
        musteri.GorusmeTarihi = UtcYap(musteri.GorusmeTarihi);
        musteri.BaslangicTarihi = UtcYap(musteri.BaslangicTarihi);
        musteri.BitisTarihi = UtcYap(musteri.BitisTarihi);
        musteri.BakimTarihi = UtcYap(musteri.BakimTarihi);
    }

    private static DateTime UtcYap(DateTime tarih)
    {
        if (tarih.Kind == DateTimeKind.Utc)
            return tarih;

        return DateTime.SpecifyKind(tarih, DateTimeKind.Utc);
    }

    private static List<DateTime> BakimTarihleriOlustur(
 DateTime ilkBakimTarihi,
 DateTime bitisTarihi,
 int periyodikBakim,
 string periyodikBakimTuru,
 string? belirliGunler = null)
    {
        List<DateTime> tarihler = new();

        // 🔥 KENDİM BELİRLEYECEĞİM
        if ((periyodikBakimTuru ?? "").ToLower().Contains("kendim"))
        {
            if (string.IsNullOrWhiteSpace(belirliGunler))
                return tarihler;

            var gunler = belirliGunler
                .Split(",", StringSplitOptions.RemoveEmptyEntries)
                .Select(x =>
                {
                    bool ok = int.TryParse(x.Trim(), out int gun);
                    return ok ? gun : 0;
                })
                .Where(x => x >= 1 && x <= 31)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            if (gunler.Count == 0)
                return tarihler;

            DateTime ay = DateTime.SpecifyKind(
     new DateTime(
         ilkBakimTarihi.Year,
         ilkBakimTarihi.Month,
         1
     ),
     DateTimeKind.Utc
 );

            while (ay <= bitisTarihi)
            {
                foreach (var gun in gunler)
                {
                    int ayinSonGunu = DateTime.DaysInMonth(ay.Year, ay.Month);

                    if (gun > ayinSonGunu)
                        continue;

                    DateTime tarih = DateTime.SpecifyKind(
    new DateTime(ay.Year, ay.Month, gun),
    DateTimeKind.Utc
);

                    if (tarih >= ilkBakimTarihi && tarih <= bitisTarihi)
                        tarihler.Add(tarih);
                }

                ay = ay.AddMonths(1);
            }

            return tarihler;
        }

        // 🔥 NORMAL SİSTEM
        if (periyodikBakim <= 0)
            return tarihler;

        DateTime tarihNormal = ilkBakimTarihi;

        while (tarihNormal <= bitisTarihi)
        {
            tarihler.Add(tarihNormal);

            tarihNormal = (periyodikBakimTuru ?? "").ToLower() switch
            {
                "gün" or "gun" => tarihNormal.AddDays(periyodikBakim),
                "hafta" => tarihNormal.AddDays(periyodikBakim * 7),
                "ay" => tarihNormal.AddMonths(periyodikBakim),
                "yıl" or "yil" => tarihNormal.AddYears(periyodikBakim),
                _ => tarihNormal.AddMonths(periyodikBakim)
            };
        }

        return tarihler;

}

    public async Task<bool> MusteriDurumDegistir(string id, string durumKodu)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return false;

        Musteri musteri = doc.ConvertTo<Musteri>();
        musteri.Id = doc.Id;

        await docRef.UpdateAsync(new Dictionary<string, object>
    {
        { "DurumKodu", durumKodu }
    });

        // 🔥 BURASI ÖNEMLİ
        if (durumKodu == "P")
        {
            await _bakimPlaniHelper.MusteriyeAitBakimlariPasifYap(musteri.MusteriNo);
        }

        return true;
    }
    public async Task<List<Musteri>> MusterileriDurumaGoreGetir(string durumKodu)
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .GetSnapshotAsync();

        List<Musteri> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                Musteri musteri = doc.ConvertTo<Musteri>();
                musteri.Id = doc.Id;
                liste.Add(musteri);
            }
        }

        return liste.Where(x => x.DurumKodu == durumKodu).ToList();
    }
    public async Task<DateTime?> SonrakiBakimTarihiGetir(int musteriNo)
    {
        QuerySnapshot snapshot = await _db.Collection("bakimPlanlari")
            .WhereEqualTo("MusteriNo", musteriNo)
            .WhereEqualTo("DurumKodu", "B") // 🔥 sadece bekleyen
            .OrderBy("BakimTarihi")
            .Limit(1)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
            return null;

        return snapshot.Documents[0].GetValue<DateTime>("BakimTarihi");
    }
}