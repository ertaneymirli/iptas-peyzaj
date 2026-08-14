using IptasPeyzajApi.Backend.Data;
using IptasPeyzajApi.Backend.Data.Entities;
using IptasPeyzajApi.Backend.Teklifler.Models;
using Microsoft.EntityFrameworkCore;

namespace IptasPeyzajApi.Backend.Teklifler.Helpers;

public class TeklifHelper
{
    private readonly IptasPeyzajDbContext _db;

    public TeklifHelper(IptasPeyzajDbContext db) => _db = db;

    public Task<List<Teklif>> TumTeklifleriGetir() => Listele(false);
    public Task<List<Teklif>> PasifTeklifleriGetir() => Listele(true);

    public async Task<Teklif?> TeklifGetir(int id)
    {
        TeklifEntity? entity = await _db.Teklifler.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return entity == null ? null : ModeleCevir(entity);
    }

    public async Task<Teklif> TeklifEkle(Teklif teklif)
    {
        TeklifEntity entity = new();
        await DegerleriYaz(entity, teklif);
        entity.DurumKodu = "B";
        entity.KayitTarihi = DateTime.UtcNow;

        _db.Teklifler.Add(entity);
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<Teklif?> DurumGuncelle(int id, string durumKodu, string islemNotu)
    {
        TeklifEntity? entity = await _db.Teklifler.FindAsync(id);
        if (entity == null) return null;

        entity.DurumKodu = durumKodu?.Trim().ToUpperInvariant() ?? "B";
        entity.IslemNotu = islemNotu ?? string.Empty;
        entity.IslemTarihi = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public Task<Teklif?> TeklifSil(int id) =>
        DurumGuncelle(id, "P", "Teklif pasife alındı.");

    public async Task<Teklif?> TeklifGuncelle(int id, Teklif teklif)
    {
        TeklifEntity? entity = await _db.Teklifler.FindAsync(id);
        if (entity == null) return null;

        await DegerleriYaz(entity, teklif);
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    private async Task<List<Teklif>> Listele(bool pasif)
    {
        List<TeklifEntity> entities = await _db.Teklifler.AsNoTracking()
            .Where(x => pasif ? x.DurumKodu == "P" : x.DurumKodu != "P")
            .OrderByDescending(x => x.KayitTarihi)
            .ToListAsync();

        return entities.Select(ModeleCevir).ToList();
    }

    private async Task DegerleriYaz(TeklifEntity entity, Teklif teklif)
    {
        entity.MusteriNo = teklif.MusteriNo;
        entity.MusteriId = teklif.MusteriId;
        entity.AdSoyad = teklif.AdSoyad?.Trim() ?? string.Empty;
        entity.Telefon = teklif.Telefon?.Trim() ?? string.Empty;
        entity.TeklifTarihi = UtcYap(teklif.TeklifTarihi);
        entity.Aciklama = teklif.Aciklama ?? string.Empty;
        entity.Tutar = teklif.Tutar;

        if (entity.MusteriId == null && teklif.MusteriNo > 0)
        {
            entity.MusteriId = await _db.Musteriler
                .Where(x => x.MusteriNo == teklif.MusteriNo)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }
    }

    private static Teklif ModeleCevir(TeklifEntity x) => new()
    {
        Id = x.Id,
        MusteriId = x.MusteriId,
        MusteriNo = x.MusteriNo,
        AdSoyad = x.AdSoyad,
        Telefon = x.Telefon,
        TeklifTarihi = x.TeklifTarihi,
        Aciklama = x.Aciklama,
        Tutar = x.Tutar,
        DurumKodu = x.DurumKodu,
        IslemNotu = x.IslemNotu,
        KayitTarihi = x.KayitTarihi,
        IslemTarihi = x.IslemTarihi
    };

    private static DateTime UtcYap(DateTime tarih) => tarih.Kind == DateTimeKind.Utc
        ? tarih
        : DateTime.SpecifyKind(tarih, DateTimeKind.Utc);
}
