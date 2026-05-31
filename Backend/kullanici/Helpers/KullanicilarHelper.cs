using Google.Cloud.Firestore;
using IptasPeyzajApi.Backend.kullanici.Model;
using System.Security.Cryptography;
using System.Text;

namespace IptasPeyzajApi.Backend.Kullanicilar.Helpers;

public class KullaniciHelper
{
    private readonly FirestoreDb _db;
    private const string CollectionName = "kullanicilar";

    public KullaniciHelper(FirestoreDb db)
    {
        _db = db;
    }

    public async Task<List<Kullanici>> TumKullanicilariGetir()
    {
        var snapshot = await _db.Collection(CollectionName).GetSnapshotAsync();

        List<Kullanici> liste = new();

        foreach (var doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                var k = doc.ConvertTo<Kullanici>();
                k.Id = doc.Id;
                k.SifreHash = "";
                liste.Add(k);
            }
        }

        return liste.OrderBy(x => x.KullaniciAdi).ToList();
    }

    public async Task<Kullanici> KullaniciEkle(Kullanici kullanici)
    {
        kullanici.KayitTarihi = DateTime.UtcNow;
        kullanici.AktifMi = true;

        if (!string.IsNullOrWhiteSpace(kullanici.Sifre))
            kullanici.SifreHash = Hashle(kullanici.Sifre);

        var docRef = await _db.Collection(CollectionName).AddAsync(kullanici);
        kullanici.Id = docRef.Id;
        kullanici.SifreHash = "";

        return kullanici;
    }

    public async Task<Kullanici?> KullaniciGuncelle(string id, Kullanici kullanici)
    {
        var docRef = _db.Collection(CollectionName).Document(id);
        var doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        var data = new Dictionary<string, object>
        {
            { "KullaniciAdi", kullanici.KullaniciAdi ?? "" },
            { "Ad", kullanici.Ad ?? "" },
            { "Soyad", kullanici.Soyad ?? "" },
            { "CepTelefonNo", kullanici.CepTelefonNo ?? "" },
            { "Mail", kullanici.Mail ?? "" },
            { "Rol", kullanici.Rol ?? "2" }
        };

        if (!string.IsNullOrWhiteSpace(kullanici.Sifre))
        {
            data.Add("SifreHash", Hashle(kullanici.Sifre));
        }

        await docRef.UpdateAsync(data);

        return await KullaniciGetir(id);
    }

    public async Task<Kullanici?> KullaniciGetir(string id)
    {
        var doc = await _db.Collection(CollectionName).Document(id).GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        var k = doc.ConvertTo<Kullanici>();
        k.Id = doc.Id;
        k.SifreHash = "";

        return k;
    }

    public async Task<Kullanici?> DurumGuncelle(string id, bool aktifMi)
    {
        var docRef = _db.Collection(CollectionName).Document(id);
        var doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "AktifMi", aktifMi }
        });

        return await KullaniciGetir(id);
    }

    private static string Hashle(string sifre)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sifre));
        return Convert.ToHexString(bytes);
    }
}