using IptasPeyzajApi.Backend.BakimPlanlari.Models;
using IptasPeyzajApi.Backend.Data;
using IptasPeyzajApi.Backend.Data.Entities;
using IptasPeyzajApi.Backend.Musteriler.Models;
using Microsoft.EntityFrameworkCore;
using SkiaSharp;

namespace IptasPeyzajApi.Backend.BakimPlanlari.Helpers;

public class BakimPlaniHelper
{
    private readonly IptasPeyzajDbContext _db;
    private readonly GoogleDriveStorage _driveStorage;

    public BakimPlaniHelper(IptasPeyzajDbContext db, GoogleDriveStorage driveStorage)
    {
        _db = db;
        _driveStorage = driveStorage;
    }

    public async Task<List<BakimPlani>> TumBakimlariGetir()
    {
        List<BakimPlaniEntity> entities = await _db.BakimPlanlari.AsNoTracking()
            .Include(x => x.Personeller)
            .OrderBy(x => x.BakimTarihi)
            .ToListAsync();
        return entities.Select(ModeleCevir).ToList();
    }

    public async Task<List<BakimPlani>> DurumaGoreGetir(string durumKodu)
    {
        string durum = durumKodu.Trim().ToUpperInvariant();
        List<BakimPlaniEntity> entities = await _db.BakimPlanlari.AsNoTracking()
            .Include(x => x.Personeller)
            .Where(x => x.DurumKodu == durum)
            .OrderBy(x => x.BakimTarihi)
            .ToListAsync();
        return entities.Select(ModeleCevir).ToList();
    }

    public async Task<BakimPlani?> BakimGetir(int id)
    {
        BakimPlaniEntity? entity = await _db.BakimPlanlari.AsNoTracking()
            .Include(x => x.Personeller)
            .FirstOrDefaultAsync(x => x.Id == id);
        return entity == null ? null : ModeleCevir(entity);
    }

    public async Task<BakimPlani> BakimEkle(BakimPlani bakim)
    {
        MusteriEntity? musteri = await _db.Musteriler.FindAsync(bakim.MusteriId);
        if (musteri == null) throw new ArgumentException("Müşteri bulunamadı.");

        BakimPlaniEntity entity = YeniBakimEntity(musteri, UtcYap(bakim.BakimTarihi));
        entity.Aciklama = bakim.Aciklama ?? string.Empty;
        _db.BakimPlanlari.Add(entity);
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<BakimPlani?> DurumGuncelle(int id, string durumKodu, string islemNotu)
    {
        BakimPlaniEntity? entity = await _db.BakimPlanlari.FindAsync(id);
        if (entity == null) return null;
        entity.DurumKodu = durumKodu.Trim().ToUpperInvariant();
        entity.IslemTarihi = DateTime.UtcNow;
        entity.IslemNotu = islemNotu ?? string.Empty;
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<BakimPlani?> Ertele(int id, DateTime yeniTarih, string islemNotu)
    {
        BakimPlaniEntity? entity = await _db.BakimPlanlari.FindAsync(id);
        if (entity == null) return null;
        entity.DurumKodu = "E";
        entity.BakimTarihi = UtcYap(yeniTarih);
        entity.IslemTarihi = DateTime.UtcNow;
        entity.IslemNotu = string.IsNullOrWhiteSpace(islemNotu)
            ? "Bakım ertelendi."
            : islemNotu;
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task MusteriIcinBakimPlanlariOlustur(
        Musteri musteri, IReadOnlyCollection<DateTime> tarihler)
    {
        if (tarihler.Count == 0) return;
        MusteriEntity? entity = await _db.Musteriler.FindAsync(musteri.Id);
        if (entity == null) throw new InvalidOperationException("Müşteri bulunamadı.");

        foreach (DateTime tarih in tarihler.Distinct())
            _db.BakimPlanlari.Add(YeniBakimEntity(entity, UtcYap(tarih)));
        await _db.SaveChangesAsync();
    }

    public async Task MusteriBakimPlaniniGuncelle(
        Musteri musteri,
        IReadOnlyCollection<DateTime> tarihler,
        string neden)
    {
        List<BakimPlaniEntity> bekleyenler = await _db.BakimPlanlari
            .Where(x => x.MusteriId == musteri.Id && x.DurumKodu == "B")
            .ToListAsync();
        foreach (BakimPlaniEntity plan in bekleyenler)
        {
            plan.DurumKodu = "I";
            plan.IslemTarihi = DateTime.UtcNow;
            plan.IslemNotu = neden;
            plan.Aciklama = neden;
        }
        await _db.SaveChangesAsync();
        await MusteriIcinBakimPlanlariOlustur(musteri, tarihler);
    }

    public async Task MusteriyeAitBakimlariPasifYap(int musteriId)
    {
        List<BakimPlaniEntity> bekleyenler = await _db.BakimPlanlari
            .Where(x => x.MusteriId == musteriId && x.DurumKodu == "B")
            .ToListAsync();
        foreach (BakimPlaniEntity plan in bekleyenler)
        {
            plan.DurumKodu = "I";
            plan.IslemTarihi = DateTime.UtcNow;
            plan.IslemNotu = "Müşteri pasif edildiği için bakım iptal edildi.";
            plan.Aciklama = "Müşteri pasif edildi.";
        }
        await _db.SaveChangesAsync();
    }

    public async Task<BakimPlani?> BakimTamamla(
        int id,
        List<int> personelIdleri,
        string islemNotu,
        IFormFile? oncesiResim,
        IFormFile? sonrasiResim)
    {
        BakimPlaniEntity? bakim = await _db.BakimPlanlari
            .Include(x => x.Personeller)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (bakim == null) return null;

        List<int> tekilPersoneller = personelIdleri.Where(x => x > 0).Distinct().ToList();
        HashSet<int> mevcutPersoneller = bakim.Personeller.Select(x => x.PersonelId).ToHashSet();
        HashSet<int> gecerliPersoneller = (await _db.Personeller
            .Where(x => tekilPersoneller.Contains(x.Id) && x.DurumKodu != "P")
            .Select(x => x.Id).ToListAsync()).ToHashSet();
        if (gecerliPersoneller.Count != tekilPersoneller.Count)
            throw new ArgumentException("Seçilen personellerden biri bulunamadı veya pasif.");

        string oncesiDriveId = await ResmiDriveaKaydet(id, "oncesi", oncesiResim);
        string sonrasiDriveId = await ResmiDriveaKaydet(id, "sonrasi", sonrasiResim);

        foreach (int personelId in tekilPersoneller.Where(x => !mevcutPersoneller.Contains(x)))
            _db.BakimPersonelleri.Add(new BakimPersonelEntity
            {
                BakimId = id,
                PersonelId = personelId
            });

        DetayEkle(id, "O", oncesiDriveId);
        DetayEkle(id, "S", sonrasiDriveId);

        bakim.DurumKodu = "T";
        bakim.IslemNotu = islemNotu ?? string.Empty;
        bakim.IslemTarihi = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return await BakimGetir(id);
    }

    public async Task<List<BakimDetay>> BakimDetaylariGetir(int bakimId)
    {
        List<BakimDetayEntity> resimler = await _db.BakimDetaylari.AsNoTracking()
            .Where(x => x.BakimId == bakimId)
            .OrderBy(x => x.Id).ToListAsync();
        List<BakimPersonelEntity> personeller = await _db.BakimPersonelleri.AsNoTracking()
            .Include(x => x.Personel)
            .Where(x => x.BakimId == bakimId)
            .OrderBy(x => x.Personel.PersonelNo).ToListAsync();

        List<BakimDetay> sonuc = new();
        if (personeller.Count > 0)
        {
            foreach (BakimPersonelEntity baglanti in personeller)
            {
                if (resimler.Count == 0)
                    sonuc.Add(PersonelDetayi(bakimId, baglanti, null));
                else
                    sonuc.AddRange(resimler.Select(resim => PersonelDetayi(bakimId, baglanti, resim)));
            }
        }
        else
        {
            sonuc.AddRange(resimler.Select(ResimDetayi));
        }
        return sonuc;
    }

    public async Task<BakimDetay?> BakimDetayGetir(int detayId)
    {
        BakimDetayEntity? entity = await _db.BakimDetaylari.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == detayId);
        return entity == null ? null : ResimDetayi(entity);
    }

    private void DetayEkle(int bakimId, string tip, string driveDosyaId)
    {
        if (string.IsNullOrWhiteSpace(driveDosyaId)) return;
        _db.BakimDetaylari.Add(new BakimDetayEntity
        {
            BakimId = bakimId,
            ResimTip = tip,
            ResimUrl = string.Empty,
            DriveDosyaId = driveDosyaId,
            LegacyKey = $"sql:{bakimId}:{tip}:{Guid.NewGuid():N}",
            KayitTarihi = DateTime.UtcNow
        });
    }

    private async Task<string> ResmiDriveaKaydet(int bakimId, string resimTipi, IFormFile? file)
    {
        if (file == null || file.Length == 0) return string.Empty;
        if (file.Length > 15 * 1024 * 1024) throw new ArgumentException("Resim en fazla 15 MB olabilir.");
        if (string.IsNullOrWhiteSpace(file.ContentType) ||
            !file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Yalnızca resim dosyası yüklenebilir.");

        using Stream input = file.OpenReadStream();
        using SKBitmap original = SKBitmap.Decode(input)
            ?? throw new ArgumentException("Resim okunamadı.");
        int genislik = Math.Min(original.Width, 1000);
        int yukseklik = Math.Max(1, (int)Math.Round((double)original.Height / original.Width * genislik));
        using SKBitmap resized = genislik == original.Width
            ? original.Copy()
            : original.Resize(new SKImageInfo(genislik, yukseklik), SKSamplingOptions.Default)
                ?? throw new InvalidOperationException("Resim hazırlanamadı.");
        using SKImage image = SKImage.FromBitmap(resized);
        using SKData data = image.Encode(SKEncodedImageFormat.Jpeg, 65)
            ?? throw new InvalidOperationException("Resim JPG biçimine dönüştürülemedi.");
        using MemoryStream output = new();
        data.SaveTo(output);
        output.Position = 0;

        string dosyaAdi = $"{bakimId}-{resimTipi}-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid():N}.jpg";
        return await _driveStorage.JpegYukleAsync(output, dosyaAdi);
    }

    private static BakimPlaniEntity YeniBakimEntity(MusteriEntity musteri, DateTime tarih) => new()
    {
        MusteriId = musteri.Id,
        MusteriNo = musteri.MusteriNo,
        AdSoyad = $"{musteri.Ad} {musteri.Soyad}".Trim(),
        Telefon = musteri.Telefon,
        BakimTarihi = tarih,
        DurumKodu = "B",
        Aciklama = "Müşteri kaydından otomatik oluşturuldu.",
        KayitTarihi = DateTime.UtcNow
    };

    private static BakimPlani ModeleCevir(BakimPlaniEntity x) => new()
    {
        Id = x.Id,
        MusteriId = x.MusteriId,
        MusteriNo = x.MusteriNo,
        AdSoyad = x.AdSoyad,
        Telefon = x.Telefon,
        BakimTarihi = x.BakimTarihi,
        DurumKodu = x.DurumKodu,
        Aciklama = x.Aciklama,
        KayitTarihi = x.KayitTarihi,
        IslemTarihi = x.IslemTarihi,
        IslemNotu = x.IslemNotu,
        PersonelIdleri = x.Personeller.Select(p => p.PersonelId).ToList()
    };

    private static BakimDetay PersonelDetayi(
        int bakimId, BakimPersonelEntity baglanti, BakimDetayEntity? resim) => new()
    {
        Id = resim?.Id ?? 0,
        BakimId = bakimId,
        PersonelNo = baglanti.Personel.PersonelNo,
        Ad = baglanti.Personel.Ad,
        Soyad = baglanti.Personel.Soyad,
        AdSoyad = $"{baglanti.Personel.Ad} {baglanti.Personel.Soyad}".Trim(),
        ResimTip = resim?.ResimTip ?? string.Empty,
        DriveDosyaId = resim?.DriveDosyaId ?? string.Empty,
        ResimUrl = resim == null ? string.Empty : ResimAdresi(resim.Id),
        KayitTarihi = resim?.KayitTarihi ?? DateTime.MinValue
    };

    private static BakimDetay ResimDetayi(BakimDetayEntity x) => new()
    {
        Id = x.Id,
        BakimId = x.BakimId,
        ResimTip = x.ResimTip,
        DriveDosyaId = x.DriveDosyaId,
        ResimUrl = string.IsNullOrWhiteSpace(x.DriveDosyaId) ? string.Empty : ResimAdresi(x.Id),
        KayitTarihi = x.KayitTarihi
    };

    private static string ResimAdresi(int detayId) =>
        $"/api/BakimPlanlari/detaylar/{detayId}/resim";

    private static DateTime UtcYap(DateTime tarih) => tarih.Kind == DateTimeKind.Utc
        ? tarih
        : DateTime.SpecifyKind(tarih, DateTimeKind.Utc);
}
