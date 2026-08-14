using IptasPeyzajApi.Backend.Data;
using IptasPeyzajApi.Backend.Data.Entities;
using IptasPeyzajApi.Backend.MusteriKullanicilari.Models;
using Microsoft.EntityFrameworkCore;

namespace IptasPeyzajApi.Backend.MusteriKullanicilari.Helpers;

public class MusteriKullanicisiHelper
{
    private readonly IptasPeyzajDbContext _db;

    public MusteriKullanicisiHelper(IptasPeyzajDbContext db) => _db = db;

    public async Task<List<MusteriKullanicisi>> TumBaglantilariGetir()
    {
        List<MusteriKullaniciEntity> entities = await _db.MusteriKullanicilari
            .AsNoTracking().OrderBy(x => x.Id).ToListAsync();
        return entities.Select(ModeleCevir).ToList();
    }

    public async Task<MusteriKullanicisi?> BaglantiGetir(int id)
    {
        MusteriKullaniciEntity? entity = await _db.MusteriKullanicilari
            .AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
        return entity == null ? null : ModeleCevir(entity);
    }

    public async Task<List<MusteriKullanicisi>> KullaniciyaGoreGetir(int kullaniciId)
    {
        List<MusteriKullaniciEntity> entities = await _db.MusteriKullanicilari
            .AsNoTracking().Where(x => x.KullaniciId == kullaniciId).ToListAsync();
        return entities.Select(ModeleCevir).ToList();
    }

    public async Task<List<MusteriKullanicisi>> MusteriyeGoreGetir(int musteriId)
    {
        List<MusteriKullaniciEntity> entities = await _db.MusteriKullanicilari
            .AsNoTracking().Where(x => x.MusteriId == musteriId).ToListAsync();
        return entities.Select(ModeleCevir).ToList();
    }

    public async Task<MusteriKullanicisi> BaglantiEkle(MusteriKullanicisi model)
    {
        await BaglantiDogrula(model.KullaniciId, model.MusteriId, null);

        MusteriKullaniciEntity entity = new()
        {
            KullaniciId = model.KullaniciId,
            MusteriId = model.MusteriId
        };
        _db.MusteriKullanicilari.Add(entity);
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<MusteriKullanicisi?> BaglantiGuncelle(
        int id, MusteriKullanicisi model)
    {
        MusteriKullaniciEntity? entity = await _db.MusteriKullanicilari.FindAsync(id);
        if (entity == null) return null;

        await BaglantiDogrula(model.KullaniciId, model.MusteriId, id);
        entity.KullaniciId = model.KullaniciId;
        entity.MusteriId = model.MusteriId;
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<bool> BaglantiSil(int id)
    {
        MusteriKullaniciEntity? entity = await _db.MusteriKullanicilari.FindAsync(id);
        if (entity == null) return false;
        _db.MusteriKullanicilari.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> KullaniciMusteriBaglantisiSil(int kullaniciId, int musteriId)
    {
        MusteriKullaniciEntity? entity = await _db.MusteriKullanicilari
            .FirstOrDefaultAsync(x => x.KullaniciId == kullaniciId && x.MusteriId == musteriId);
        if (entity == null) return false;
        _db.MusteriKullanicilari.Remove(entity);
        await _db.SaveChangesAsync();
        return true;
    }

    private async Task BaglantiDogrula(int kullaniciId, int musteriId, int? haricId)
    {
        if (kullaniciId <= 0) throw new ArgumentException("Kullanıcı ID geçersiz.");
        if (musteriId <= 0) throw new ArgumentException("Müşteri ID geçersiz.");
        if (!await _db.Kullanicilar.AnyAsync(x => x.Id == kullaniciId))
            throw new ArgumentException("Kullanıcı bulunamadı.");
        if (!await _db.Musteriler.AnyAsync(x => x.Id == musteriId))
            throw new ArgumentException("Müşteri bulunamadı.");

        bool varMi = await _db.MusteriKullanicilari.AnyAsync(x =>
            x.KullaniciId == kullaniciId && x.MusteriId == musteriId &&
            (!haricId.HasValue || x.Id != haricId.Value));
        if (varMi)
            throw new InvalidOperationException("Bu kullanıcı ile müşteri zaten eşleştirilmiş.");
    }

    private static MusteriKullanicisi ModeleCevir(MusteriKullaniciEntity x) => new()
    {
        Id = x.Id,
        KullaniciId = x.KullaniciId,
        MusteriId = x.MusteriId
    };
}
