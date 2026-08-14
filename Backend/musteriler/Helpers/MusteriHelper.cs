using IptasPeyzajApi.Backend.BakimPlanlari.Helpers;
using IptasPeyzajApi.Backend.Data;
using IptasPeyzajApi.Backend.Data.Entities;
using IptasPeyzajApi.Backend.Musteriler.Models;
using Microsoft.EntityFrameworkCore;

namespace IptasPeyzajApi.Backend.Musteriler.Helpers;

public class MusteriHelper
{
    private readonly IptasPeyzajDbContext _db;
    private readonly BakimPlaniHelper _bakimPlaniHelper;

    public MusteriHelper(IptasPeyzajDbContext db, BakimPlaniHelper bakimPlaniHelper)
    {
        _db = db;
        _bakimPlaniHelper = bakimPlaniHelper;
    }

    public async Task<List<Musteri>> TumMusterileriGetir()
    {
        List<MusteriEntity> entities = await _db.Musteriler.AsNoTracking()
            .Where(x => x.DurumKodu != "P")
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return await ModelleriHazirla(entities);
    }

    public async Task<Musteri?> MusteriGetir(int id)
    {
        MusteriEntity? entity = await _db.Musteriler.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null) return null;
        return (await ModelleriHazirla(new List<MusteriEntity> { entity })).Single();
    }

    public async Task<Musteri?> MusteriNodanGetir(int musteriNo)
    {
        MusteriEntity? entity = await _db.Musteriler.AsNoTracking()
            .FirstOrDefaultAsync(x => x.MusteriNo == musteriNo);
        if (entity == null) return null;
        return (await ModelleriHazirla(new List<MusteriEntity> { entity })).Single();
    }

    public async Task<Musteri> MusteriEkle(Musteri musteri)
    {
        TcKontrol(musteri.Tc);
        TarihleriUtcYap(musteri);

        MusteriEntity entity = new()
        {
            MusteriNo = (await _db.Musteriler.MaxAsync(x => (int?)x.MusteriNo) ?? 0) + 1,
            KayitTarihi = DateTime.UtcNow,
            DurumKodu = "A"
        };
        DegerleriYaz(entity, musteri);

        _db.Musteriler.Add(entity);
        await _db.SaveChangesAsync();

        musteri.Id = entity.Id;
        musteri.MusteriNo = entity.MusteriNo;
        musteri.KayitTarihi = entity.KayitTarihi;
        musteri.DurumKodu = entity.DurumKodu;

        List<DateTime> tarihler = BakimTarihleriOlustur(musteri);
        await _bakimPlaniHelper.MusteriIcinBakimPlanlariOlustur(musteri, tarihler);
        return musteri;
    }

    public async Task<Musteri?> MusteriGuncelle(int id, Musteri musteri)
    {
        TcKontrol(musteri.Tc);
        MusteriEntity? entity = await _db.Musteriler.FindAsync(id);
        if (entity == null) return null;

        TarihleriUtcYap(musteri);
        DateTime? eskiIlkBakim = await SonrakiBakimTarihiGetir(entity.MusteriNo);
        bool planDegisti =
            entity.BitisTarihi != musteri.BitisTarihi ||
            entity.PeriyodikBakim != musteri.PeriyodikBakim ||
            entity.PeriyodikBakimTuru != (musteri.PeriyodikBakimTuru ?? string.Empty) ||
            entity.BelirliGunler != (musteri.BelirliGunler ?? string.Empty) ||
            (eskiIlkBakim?.Date ?? DateTime.MinValue) != musteri.BakimTarihi.Date;

        DegerleriYaz(entity, musteri);
        await _db.SaveChangesAsync();

        musteri.Id = entity.Id;
        musteri.MusteriNo = entity.MusteriNo;
        musteri.KayitTarihi = entity.KayitTarihi;
        musteri.DurumKodu = entity.DurumKodu;

        if (planDegisti)
        {
            await _bakimPlaniHelper.MusteriBakimPlaniniGuncelle(
                musteri,
                BakimTarihleriOlustur(musteri),
                "Müşteri bakım tarihi veya periyodu değiştiği için eski bekleyen bakım planı iptal edildi.");
        }

        return musteri;
    }

    public async Task<bool> MusteriSil(int id) => await MusteriDurumDegistir(id, "P");

    public async Task<bool> MusteriDurumDegistir(int id, string durumKodu)
    {
        MusteriEntity? entity = await _db.Musteriler.FindAsync(id);
        if (entity == null) return false;

        entity.DurumKodu = durumKodu.Trim().ToUpperInvariant();
        await _db.SaveChangesAsync();
        if (entity.DurumKodu == "P")
            await _bakimPlaniHelper.MusteriyeAitBakimlariPasifYap(entity.Id);
        return true;
    }

    public async Task<List<Musteri>> MusterileriDurumaGoreGetir(string durumKodu)
    {
        string durum = durumKodu.Trim().ToUpperInvariant();
        List<MusteriEntity> entities = await _db.Musteriler.AsNoTracking()
            .Where(x => x.DurumKodu == durum)
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();
        return await ModelleriHazirla(entities);
    }

    public Task<DateTime?> SonrakiBakimTarihiGetir(int musteriNo) =>
        _db.BakimPlanlari.AsNoTracking()
            .Where(x => x.MusteriNo == musteriNo && x.DurumKodu == "B")
            .OrderBy(x => x.BakimTarihi)
            .Select(x => (DateTime?)x.BakimTarihi)
            .FirstOrDefaultAsync();

    private async Task<List<Musteri>> ModelleriHazirla(List<MusteriEntity> entities)
    {
        int[] ids = entities.Select(x => x.Id).ToArray();
        Dictionary<int, DateTime> sonrakiTarihler = await _db.BakimPlanlari.AsNoTracking()
            .Where(x => ids.Contains(x.MusteriId) && x.DurumKodu == "B")
            .GroupBy(x => x.MusteriId)
            .Select(g => new { MusteriId = g.Key, Tarih = g.Min(x => x.BakimTarihi) })
            .ToDictionaryAsync(x => x.MusteriId, x => x.Tarih);

        return entities.Select(x => ModeleCevir(
            x,
            sonrakiTarihler.TryGetValue(x.Id, out DateTime tarih) ? tarih : default)).ToList();
    }

    private static Musteri ModeleCevir(MusteriEntity x, DateTime bakimTarihi) => new()
    {
        Id = x.Id,
        MusteriNo = x.MusteriNo,
        Ad = x.Ad,
        Soyad = x.Soyad,
        Tc = x.Tc,
        DogumTarihi = x.DogumTarihi,
        Cinsiyet = x.Cinsiyet,
        Telefon = x.Telefon,
        CaddeSokak = x.CaddeSokak,
        Mahalle = x.Mahalle,
        No = x.No,
        Daire = x.Daire,
        Sehir = x.Sehir,
        Adres = x.Adres,
        MekanTipi = x.MekanTipi,
        SozlesmeTarihi = x.SozlesmeTarihi,
        GorusmeTarihi = x.GorusmeTarihi,
        BaslangicTarihi = x.BaslangicTarihi,
        BitisTarihi = x.BitisTarihi,
        BakimTarihi = bakimTarihi,
        PeriyodikBakim = x.PeriyodikBakim,
        PeriyodikBakimTuru = x.PeriyodikBakimTuru,
        BelirliGunler = x.BelirliGunler,
        Aciklama = x.Aciklama,
        KayitTarihi = x.KayitTarihi,
        DurumKodu = x.DurumKodu
    };

    private static void DegerleriYaz(MusteriEntity x, Musteri m)
    {
        x.Ad = m.Ad?.Trim() ?? string.Empty;
        x.Soyad = m.Soyad?.Trim() ?? string.Empty;
        x.Tc = m.Tc?.Trim() ?? string.Empty;
        x.DogumTarihi = m.DogumTarihi;
        x.Cinsiyet = m.Cinsiyet?.Trim() ?? string.Empty;
        x.Telefon = m.Telefon?.Trim() ?? string.Empty;
        x.CaddeSokak = m.CaddeSokak?.Trim() ?? string.Empty;
        x.Mahalle = m.Mahalle?.Trim() ?? string.Empty;
        x.No = m.No?.Trim() ?? string.Empty;
        x.Daire = m.Daire?.Trim() ?? string.Empty;
        x.Sehir = m.Sehir?.Trim() ?? string.Empty;
        x.Adres = string.IsNullOrWhiteSpace(m.Adres)
            ? $"{x.Mahalle} Mah. {x.CaddeSokak} No:{x.No} Daire:{x.Daire} {x.Sehir}".Trim()
            : m.Adres.Trim();
        x.MekanTipi = m.MekanTipi?.Trim() ?? string.Empty;
        x.SozlesmeTarihi = m.SozlesmeTarihi;
        x.GorusmeTarihi = m.GorusmeTarihi;
        x.BaslangicTarihi = m.BaslangicTarihi;
        x.BitisTarihi = m.BitisTarihi;
        x.PeriyodikBakim = m.PeriyodikBakim;
        x.PeriyodikBakimTuru = m.PeriyodikBakimTuru?.Trim() ?? string.Empty;
        x.BelirliGunler = m.BelirliGunler?.Trim() ?? string.Empty;
        x.Aciklama = m.Aciklama ?? string.Empty;
    }

    private static List<DateTime> BakimTarihleriOlustur(Musteri m)
    {
        List<DateTime> tarihler = new();
        DateTime ilk = UtcYap(m.BakimTarihi);
        DateTime bitis = UtcYap(m.BitisTarihi);
        string tur = (m.PeriyodikBakimTuru ?? string.Empty).ToLowerInvariant();

        if (tur.Contains("kendim"))
        {
            int[] gunler = (m.BelirliGunler ?? string.Empty)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => int.TryParse(x.Trim(), out int gun) ? gun : 0)
                .Where(x => x is >= 1 and <= 31).Distinct().OrderBy(x => x).ToArray();
            DateTime ay = new(ilk.Year, ilk.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            while (ay <= bitis)
            {
                foreach (int gun in gunler)
                {
                    if (gun > DateTime.DaysInMonth(ay.Year, ay.Month)) continue;
                    DateTime tarih = new(ay.Year, ay.Month, gun, 0, 0, 0, DateTimeKind.Utc);
                    if (tarih >= ilk && tarih <= bitis) tarihler.Add(tarih);
                }
                ay = ay.AddMonths(1);
            }
            return tarihler;
        }

        if (m.PeriyodikBakim <= 0 || ilk > bitis) return tarihler;
        for (DateTime tarih = ilk; tarih <= bitis;)
        {
            tarihler.Add(tarih);
            tarih = tur switch
            {
                "gün" or "gun" => tarih.AddDays(m.PeriyodikBakim),
                "hafta" => tarih.AddDays(m.PeriyodikBakim * 7),
                "ay" => tarih.AddMonths(m.PeriyodikBakim),
                "yıl" or "yil" => tarih.AddYears(m.PeriyodikBakim),
                _ => tarih.AddMonths(m.PeriyodikBakim)
            };
        }
        return tarihler;
    }

    private static void TcKontrol(string? tc)
    {
        if (!string.IsNullOrEmpty(tc) && (tc.Length != 11 || !tc.All(char.IsDigit)))
            throw new ArgumentException("TC Kimlik No 11 haneli ve sadece rakam olmalıdır.");
    }

    private static void TarihleriUtcYap(Musteri m)
    {
        m.DogumTarihi = UtcYap(m.DogumTarihi);
        m.SozlesmeTarihi = UtcYap(m.SozlesmeTarihi);
        m.GorusmeTarihi = UtcYap(m.GorusmeTarihi);
        m.BaslangicTarihi = UtcYap(m.BaslangicTarihi);
        m.BitisTarihi = UtcYap(m.BitisTarihi);
        m.BakimTarihi = UtcYap(m.BakimTarihi);
    }

    private static DateTime UtcYap(DateTime tarih) => tarih.Kind == DateTimeKind.Utc
        ? tarih
        : DateTime.SpecifyKind(tarih, DateTimeKind.Utc);
}
