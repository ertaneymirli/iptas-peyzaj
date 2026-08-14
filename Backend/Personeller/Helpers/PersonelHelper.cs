using IptasPeyzajApi.Backend.Data;
using IptasPeyzajApi.Backend.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace IptasPeyzajApi.Backend.Personeller.Helpers;

public class PersonelHelper
{
    private readonly IptasPeyzajDbContext _db;

    public PersonelHelper(IptasPeyzajDbContext db) => _db = db;

    public async Task<List<Personel>> TumPersonelleriGetir()
    {
        List<PersonelEntity> entities = await _db.Personeller.AsNoTracking()
            .Where(x => x.DurumKodu != "P")
            .OrderBy(x => x.PersonelNo)
            .ToListAsync();

        return entities.Select(ModeleCevir).ToList();
    }

    public async Task<Personel?> PersonelGetir(int id)
    {
        PersonelEntity? entity = await _db.Personeller.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return entity == null ? null : ModeleCevir(entity);
    }

    public async Task<Personel> PersonelEkle(Personel personel)
    {
        int yeniNo = personel.PersonelNo > 0
            ? personel.PersonelNo
            : (await _db.Personeller.MaxAsync(x => (int?)x.PersonelNo) ?? 0) + 1;

        PersonelEntity entity = new()
        {
            PersonelNo = yeniNo,
            EskiPersonelId = 0,
            Ad = personel.Ad?.Trim() ?? string.Empty,
            Soyad = personel.Soyad?.Trim() ?? string.Empty,
            Telefon = personel.Telefon?.Trim() ?? string.Empty,
            Gorev = personel.Gorev?.Trim() ?? string.Empty,
            DurumKodu = "A",
            KayitTarihi = DateTime.UtcNow
        };

        _db.Personeller.Add(entity);
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<Personel?> PersonelGuncelle(int id, Personel personel)
    {
        PersonelEntity? entity = await _db.Personeller.FindAsync(id);
        if (entity == null) return null;

        entity.PersonelNo = personel.PersonelNo > 0 ? personel.PersonelNo : entity.PersonelNo;
        entity.Ad = personel.Ad?.Trim() ?? string.Empty;
        entity.Soyad = personel.Soyad?.Trim() ?? string.Empty;
        entity.Telefon = personel.Telefon?.Trim() ?? string.Empty;
        entity.Gorev = personel.Gorev?.Trim() ?? string.Empty;

        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<Personel?> DurumGuncelle(int id, string durumKodu)
    {
        PersonelEntity? entity = await _db.Personeller.FindAsync(id);
        if (entity == null) return null;

        entity.DurumKodu = string.IsNullOrWhiteSpace(durumKodu)
            ? entity.DurumKodu
            : durumKodu.Trim().ToUpperInvariant();
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<List<Personel>> DurumaGoreGetir(string durumKodu)
    {
        string durum = durumKodu.Trim().ToUpperInvariant();
        List<PersonelEntity> entities = await _db.Personeller.AsNoTracking()
            .Where(x => x.DurumKodu == durum)
            .OrderBy(x => x.PersonelNo)
            .ToListAsync();

        return entities.Select(ModeleCevir).ToList();
    }

    private static Personel ModeleCevir(PersonelEntity x) => new()
    {
        Id = x.Id,
        PersonelNo = x.PersonelNo,
        Ad = x.Ad,
        Soyad = x.Soyad,
        Telefon = x.Telefon,
        Gorev = x.Gorev,
        DurumKodu = x.DurumKodu,
        KayitTarihi = x.KayitTarihi
    };
}
