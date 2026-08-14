using IptasPeyzajApi.Backend.Data;
using IptasPeyzajApi.Backend.Data.Entities;
using IptasPeyzajApi.Backend.kullanici.Model;
using IptasPeyzajApi.Backend.kullanici.Model.DTO;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace IptasPeyzajApi.Backend.Kullanicilar.Helpers;

public class KullaniciHelper
{
    private readonly IptasPeyzajDbContext _db;

    public KullaniciHelper(IptasPeyzajDbContext db) => _db = db;

    public async Task<List<Kullanici>> TumKullanicilariGetir()
    {
        List<KullaniciEntity> entities = await _db.Kullanicilar.AsNoTracking()
            .OrderByDescending(x => x.KayitTarihi).ToListAsync();
        return entities.Select(ModeleCevir).ToList();
    }

    public async Task<Kullanici?> KullaniciGetir(int id)
    {
        KullaniciEntity? entity = await _db.Kullanicilar.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id);
        return entity == null ? null : ModeleCevir(entity);
    }

    public async Task<Kullanici> KullaniciEkle(KullaniciCreateDto dto)
    {
        string kullaniciAdi = dto.KullaniciAdi.Trim();
        if (await _db.Kullanicilar.AnyAsync(x => x.KullaniciAdi == kullaniciAdi))
            throw new InvalidOperationException("Bu kullanıcı adı zaten kayıtlı.");
        if (string.IsNullOrWhiteSpace(dto.Sifre))
            throw new ArgumentException("Şifre boş olamaz.");

        KullaniciEntity entity = new()
        {
            KullaniciAdi = kullaniciAdi,
            Ad = dto.Ad?.Trim() ?? string.Empty,
            Soyad = dto.Soyad?.Trim() ?? string.Empty,
            DogumTarihi = UtcYap(dto.DogumTarihi),
            Tc = dto.Tc?.Trim() ?? string.Empty,
            TelefonNo = dto.TelefonNo?.Trim() ?? string.Empty,
            CepTelefonNo = dto.CepTelefonNo?.Trim() ?? string.Empty,
            Adres = dto.Adres?.Trim() ?? string.Empty,
            Mail = dto.Mail?.Trim() ?? string.Empty,
            SifreHash = Hashle(dto.Sifre),
            Rol = dto.Rol?.Trim() ?? "2",
            AktifMi = true,
            KayitTarihi = DateTime.UtcNow
        };

        _db.Kullanicilar.Add(entity);
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<Kullanici?> KullaniciGuncelle(int id, KullaniciUpdateDto dto)
    {
        KullaniciEntity? entity = await _db.Kullanicilar.FindAsync(id);
        if (entity == null) return null;

        string kullaniciAdi = dto.KullaniciAdi.Trim();
        if (await _db.Kullanicilar.AnyAsync(x => x.Id != id && x.KullaniciAdi == kullaniciAdi))
            throw new InvalidOperationException("Bu kullanıcı adı zaten kayıtlı.");

        entity.KullaniciAdi = kullaniciAdi;
        entity.Ad = dto.Ad?.Trim() ?? string.Empty;
        entity.Soyad = dto.Soyad?.Trim() ?? string.Empty;
        entity.DogumTarihi = UtcYap(dto.DogumTarihi);
        entity.Tc = dto.Tc?.Trim() ?? string.Empty;
        entity.TelefonNo = dto.TelefonNo?.Trim() ?? string.Empty;
        entity.CepTelefonNo = dto.CepTelefonNo?.Trim() ?? string.Empty;
        entity.Adres = dto.Adres?.Trim() ?? string.Empty;
        entity.Mail = dto.Mail?.Trim() ?? string.Empty;
        entity.Rol = dto.Rol?.Trim() ?? "2";
        entity.AktifMi = dto.AktifMi;
        if (!string.IsNullOrWhiteSpace(dto.Sifre))
            entity.SifreHash = Hashle(dto.Sifre);

        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<Kullanici?> DurumGuncelle(int id, bool aktifMi)
    {
        KullaniciEntity? entity = await _db.Kullanicilar.FindAsync(id);
        if (entity == null) return null;
        entity.AktifMi = aktifMi;
        await _db.SaveChangesAsync();
        return ModeleCevir(entity);
    }

    public async Task<Kullanici?> GirisYap(LoginDto dto)
    {
        string ad = dto.KullaniciAdi.Trim();
        string hash = Hashle(dto.Sifre);
        KullaniciEntity? entity = await _db.Kullanicilar.AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.KullaniciAdi == ad && x.SifreHash == hash && x.AktifMi);
        return entity == null ? null : ModeleCevir(entity);
    }

    private static Kullanici ModeleCevir(KullaniciEntity x) => new()
    {
        Id = x.Id,
        KullaniciAdi = x.KullaniciAdi,
        Ad = x.Ad,
        Soyad = x.Soyad,
        DogumTarihi = x.DogumTarihi,
        Tc = x.Tc,
        TelefonNo = x.TelefonNo,
        CepTelefonNo = x.CepTelefonNo,
        Adres = x.Adres,
        Mail = x.Mail,
        SifreHash = string.Empty,
        Rol = x.Rol,
        AktifMi = x.AktifMi,
        KayitTarihi = x.KayitTarihi
    };

    private static string Hashle(string sifre)
    {
        using SHA256 sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(Encoding.UTF8.GetBytes(sifre)));
    }

    private static DateTime? UtcYap(DateTime? tarih) => tarih == null
        ? null
        : tarih.Value.Kind == DateTimeKind.Utc
            ? tarih
            : DateTime.SpecifyKind(tarih.Value, DateTimeKind.Utc);
}
