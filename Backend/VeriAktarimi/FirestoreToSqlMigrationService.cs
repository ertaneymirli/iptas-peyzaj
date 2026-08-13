using Google.Cloud.Firestore;
using IptasPeyzajApi.Backend.BakimPlanlari.Models;
using IptasPeyzajApi.Backend.Data;
using IptasPeyzajApi.Backend.Data.Entities;
using IptasPeyzajApi.Backend.kullanici.Model;
using IptasPeyzajApi.Backend.Models;
using IptasPeyzajApi.Backend.Musteriler.Models;
using IptasPeyzajApi.Backend.MusteriKullanicilari.Models;
using IptasPeyzajApi.Backend.Teklifler.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace IptasPeyzajApi.Backend.VeriAktarimi;

/// <summary>
/// Firestore verilerini Azure SQL'e bir kez veya tekrar güvenle aktarır.
/// Firestore kayıtlarını silmez ve mevcut uygulamanın Firestore kullanımını değiştirmez.
/// </summary>
public sealed class FirestoreToSqlMigrationService
{
    private readonly FirestoreDb _firestore;
    private readonly IptasPeyzajDbContext _sql;

    public FirestoreToSqlMigrationService(
        FirestoreDb firestore,
        IptasPeyzajDbContext sql)
    {
        _firestore = firestore;
        _sql = sql;
    }

    public async Task<VeriAktarimSonucu> AktarAsync(
        CancellationToken cancellationToken = default)
    {
        var sonuc = new VeriAktarimSonucu();

        // Firestore okumaları SQL transaction'ından önce tamamlanır.
        var musteriler = await OkuAsync<Musteri>("musteriler", cancellationToken);
        var kullanicilar = await OkuAsync<Kullanici>("kullanicilar", cancellationToken);
        var personeller = await OkuAsync<Personel>("Personeller", cancellationToken);
        var bakimlar = await OkuAsync<BakimPlani>("bakimPlanlari", cancellationToken);
        var bakimDetaylari = await OkuAsync<BakimDetay>("bakimDetaylari", cancellationToken);
        var teklifler = await OkuAsync<Teklif>("teklifler", cancellationToken);
        var musteriKullanicilari = await OkuAsync<MusteriKullanicisi>(
            "musteriKullanicilari",
            cancellationToken);
        var isler = await OkuAsync<Isler>("isler", cancellationToken);
        var yapilacakIsler = await OkuAsync<YapilacakIs>(
            "yapilacakIsler",
            cancellationToken);
        var yapilmayacakIsler = await OkuAsync<YapilmayacakIs>(
            "yapilmayacakIsler",
            cancellationToken);



        await MusterileriAktar(musteriler, sonuc, cancellationToken);
        await KullanicilariAktar(kullanicilar, sonuc, cancellationToken);
        await PersonelleriAktar(personeller, sonuc, cancellationToken);
        await BakimlariAktar(bakimlar, sonuc, cancellationToken);
        await BakimDetayVePersonelleriniAktar(
            bakimlar,
            bakimDetaylari,
            sonuc,
            cancellationToken);
        await TeklifleriAktar(teklifler, sonuc, cancellationToken);
        await MusteriKullanicilariniAktar(
            musteriKullanicilari,
            sonuc,
            cancellationToken);
        await IsleriAktar(
            isler,
            yapilacakIsler,
            yapilmayacakIsler,
            sonuc,
            cancellationToken);

      

        sonuc.SqlToplamlari = await SqlSayilariniGetir(cancellationToken);
        sonuc.Basarili = true;
        return sonuc;
    }

    public async Task<Dictionary<string, int>> SqlSayilariniGetir(
        CancellationToken cancellationToken = default)
    {
        return new Dictionary<string, int>
        {
            ["Musteriler"] = await _sql.Musteriler.CountAsync(cancellationToken),
            ["Kullanicilar"] = await _sql.Kullanicilar.CountAsync(cancellationToken),
            ["Personeller"] = await _sql.Personeller.CountAsync(cancellationToken),
            ["BakimPlanlari"] = await _sql.BakimPlanlari.CountAsync(cancellationToken),
            ["BakimDetaylari"] = await _sql.BakimDetaylari.CountAsync(cancellationToken),
            ["BakimPersonelleri"] = await _sql.BakimPersonelleri.CountAsync(cancellationToken),
            ["Teklifler"] = await _sql.Teklifler.CountAsync(cancellationToken),
            ["MusteriKullanicilari"] = await _sql.MusteriKullanicilari.CountAsync(cancellationToken),
            ["Isler"] = await _sql.Isler.CountAsync(cancellationToken),
            ["YapilacakIsler"] = await _sql.YapilacakIsler.CountAsync(cancellationToken),
            ["YapilmayacakIsler"] = await _sql.YapilmayacakIsler.CountAsync(cancellationToken)
        };
    }

    private async Task MusterileriAktar(
        IReadOnlyList<FirestoreKaydi<Musteri>> kaynak,
        VeriAktarimSonucu sonuc,
        CancellationToken ct)
    {
        var mevcutListe = await _sql.Musteriler.ToListAsync(ct);

        var mevcut = mevcutListe
            .Where(x => x.FirestoreId != null)
            .ToDictionary(x => x.FirestoreId!, StringComparer.Ordinal);

        var mevcutMusteriNolari = mevcutListe
            .GroupBy(x => x.MusteriNo)
            .ToDictionary(x => x.Key, x => x.First());
        var tekilKaynak = kaynak
    .GroupBy(x => x.Veri.MusteriNo)
    .Select(grup => grup
        .OrderBy(x => x.Veri.DurumKodu == "P" ? 1 : 0)
        .ThenByDescending(x => x.Veri.KayitTarihi)
        .First())
    .ToList();

        foreach (var kayit in tekilKaynak)
        {
            if (!mevcut.TryGetValue(kayit.Id, out var hedef))
            {
                if (mevcutMusteriNolari.TryGetValue(
                    kayit.Veri.MusteriNo,
                    out hedef))
                {
                    hedef.FirestoreId = kayit.Id;
                    sonuc.Guncellendi("Musteriler");
                }
                else
                {
                    hedef = new MusteriEntity
                    {
                        FirestoreId = kayit.Id
                    };

                    _sql.Musteriler.Add(hedef);
                    mevcutMusteriNolari[kayit.Veri.MusteriNo] = hedef;
                    sonuc.Eklendi("Musteriler");
                }

                mevcut[kayit.Id] = hedef;
            }
            else
            {
                sonuc.Guncellendi("Musteriler");
            }

            var x = kayit.Veri;
            hedef.MusteriNo = x.MusteriNo;
            hedef.Ad = x.Ad;
            hedef.Soyad = x.Soyad;
            hedef.Tc = x.Tc;
            hedef.DogumTarihi = x.DogumTarihi;
            hedef.Cinsiyet = x.Cinsiyet;
            hedef.Telefon = x.Telefon;
            hedef.CaddeSokak = x.CaddeSokak;
            hedef.Mahalle = x.Mahalle;
            hedef.No = x.No;
            hedef.Daire = x.Daire;
            hedef.Sehir = x.Sehir;
            hedef.Adres = x.Adres;
            hedef.MekanTipi = x.MekanTipi;
            hedef.SozlesmeTarihi = x.SozlesmeTarihi;
            hedef.GorusmeTarihi = x.GorusmeTarihi;
            hedef.BaslangicTarihi = x.BaslangicTarihi;
            hedef.BitisTarihi = x.BitisTarihi;
            hedef.PeriyodikBakim = x.PeriyodikBakim;
            hedef.PeriyodikBakimTuru = x.PeriyodikBakimTuru;
            hedef.BelirliGunler = x.BelirliGunler;
            hedef.Aciklama = x.Aciklama;
            hedef.KayitTarihi = x.KayitTarihi;
            hedef.DurumKodu = x.DurumKodu;

            // x.BakimTarihi ve x.BakimTarihleri özellikle aktarılmaz.
            // Bakım tarihleri BakimPlanlari tablosunda satır olarak tutulur.
        }

        await _sql.SaveChangesAsync(ct);
    }

    private async Task KullanicilariAktar(
        IReadOnlyList<FirestoreKaydi<Kullanici>> kaynak,
        VeriAktarimSonucu sonuc,
        CancellationToken ct)
    {
        var mevcut = await _sql.Kullanicilar
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);

        foreach (var kayit in kaynak)
        {
            if (!mevcut.TryGetValue(kayit.Id, out var hedef))
            {
                hedef = new KullaniciEntity { FirestoreId = kayit.Id };
                _sql.Kullanicilar.Add(hedef);
                mevcut[kayit.Id] = hedef;
                sonuc.Eklendi("Kullanicilar");
            }
            else
            {
                sonuc.Guncellendi("Kullanicilar");
            }

            var x = kayit.Veri;
            hedef.KullaniciAdi = x.KullaniciAdi;
            hedef.Ad = x.Ad;
            hedef.Soyad = x.Soyad;
            hedef.DogumTarihi = x.DogumTarihi;
            hedef.Tc = x.Tc;
            hedef.TelefonNo = x.TelefonNo;
            hedef.CepTelefonNo = x.CepTelefonNo;
            hedef.Adres = x.Adres;
            hedef.Mail = x.Mail;
            hedef.SifreHash = x.SifreHash;
            hedef.Rol = x.Rol;
            hedef.AktifMi = x.AktifMi;
            hedef.KayitTarihi = x.KayitTarihi;
        }

        await _sql.SaveChangesAsync(ct);
    }

    private async Task PersonelleriAktar(
        IReadOnlyList<FirestoreKaydi<Personel>> kaynak,
        VeriAktarimSonucu sonuc,
        CancellationToken ct)
    {
        var mevcut = await _sql.Personeller
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);

        foreach (var kayit in kaynak)
        {
            if (!mevcut.TryGetValue(kayit.Id, out var hedef))
            {
                hedef = new PersonelEntity { FirestoreId = kayit.Id };
                _sql.Personeller.Add(hedef);
                mevcut[kayit.Id] = hedef;
                sonuc.Eklendi("Personeller");
            }
            else
            {
                sonuc.Guncellendi("Personeller");
            }

            var x = kayit.Veri;
            hedef.EskiPersonelId = x.Id;
            hedef.PersonelNo = x.PersonelNo;
            hedef.Ad = x.Ad;
            hedef.Soyad = x.Soyad;
            hedef.Telefon = x.Telefon;
            hedef.Gorev = x.Gorev;
            hedef.DurumKodu = x.DurumKodu;
            hedef.KayitTarihi = x.KayitTarihi;
        }

        await _sql.SaveChangesAsync(ct);
    }

    private async Task BakimlariAktar(
        IReadOnlyList<FirestoreKaydi<BakimPlani>> kaynak,
        VeriAktarimSonucu sonuc,
        CancellationToken ct)
    {
        var musteriIdMap = await _sql.Musteriler
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);
        var musteriNoMap = (await _sql.Musteriler.ToListAsync(ct))
            .GroupBy(x => x.MusteriNo)
            .ToDictionary(x => x.Key, x => x.First());
        var mevcut = await _sql.BakimPlanlari
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);

        foreach (var kayit in kaynak)
        {
            var x = kayit.Veri;
            MusteriEntity? musteri = null;
            if (!string.IsNullOrWhiteSpace(x.MusteriId))
                musteriIdMap.TryGetValue(x.MusteriId, out musteri);
            if (musteri == null)
                musteriNoMap.TryGetValue(x.MusteriNo, out musteri);

            if (musteri == null)
            {
                sonuc.Eslesmedi("BakimPlanlari", kayit.Id);
                continue;
            }

            if (!mevcut.TryGetValue(kayit.Id, out var hedef))
            {
                hedef = new BakimPlaniEntity { FirestoreId = kayit.Id };
                _sql.BakimPlanlari.Add(hedef);
                mevcut[kayit.Id] = hedef;
                sonuc.Eklendi("BakimPlanlari");
            }
            else
            {
                sonuc.Guncellendi("BakimPlanlari");
            }

            hedef.MusteriId = musteri.Id;
            hedef.MusteriNo = x.MusteriNo;
            hedef.AdSoyad = x.AdSoyad;
            hedef.Telefon = x.Telefon;
            hedef.BakimTarihi = x.BakimTarihi;
            hedef.DurumKodu = x.DurumKodu;
            hedef.Aciklama = x.Aciklama;
            hedef.KayitTarihi = x.KayitTarihi;
            hedef.IslemTarihi = x.IslemTarihi;
            hedef.IslemNotu = x.IslemNotu;
        }

        await _sql.SaveChangesAsync(ct);
    }

    private async Task BakimDetayVePersonelleriniAktar(
        IReadOnlyList<FirestoreKaydi<BakimPlani>> bakimKaynak,
        IReadOnlyList<FirestoreKaydi<BakimDetay>> detayKaynak,
        VeriAktarimSonucu sonuc,
        CancellationToken ct)
    {
        var bakimMap = await _sql.BakimPlanlari
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);
        var personelListesi = await _sql.Personeller.ToListAsync(ct);
        var eskiPersonelMap = personelListesi
            .Where(x => x.EskiPersonelId > 0)
            .GroupBy(x => x.EskiPersonelId)
            .ToDictionary(x => x.Key, x => x.First());
        var personelNoMap = personelListesi
            .Where(x => x.PersonelNo > 0)
            .GroupBy(x => x.PersonelNo)
            .ToDictionary(x => x.Key, x => x.First());

        var mevcutBaglantilar = (await _sql.BakimPersonelleri.ToListAsync(ct))
            .Select(x => (x.BakimId, x.PersonelId))
            .ToHashSet();
        var mevcutDetayAnahtarlari = (await _sql.BakimDetaylari
                .Select(x => x.LegacyKey)
                .ToListAsync(ct))
            .ToHashSet(StringComparer.Ordinal);

        PersonelEntity? PersonelBul(int eskiId)
        {
            if (eskiPersonelMap.TryGetValue(eskiId, out var p)) return p;
            return personelNoMap.TryGetValue(eskiId, out p) ? p : null;
        }

        void BaglantiEkle(BakimPlaniEntity bakim, int eskiPersonelId, string kaynakId)
        {
            if (eskiPersonelId <= 0) return;
            var personel = PersonelBul(eskiPersonelId);
            if (personel == null)
            {
                sonuc.Eslesmedi("BakimPersonelleri", kaynakId + ":" + eskiPersonelId);
                return;
            }

            if (!mevcutBaglantilar.Add((bakim.Id, personel.Id))) return;
            _sql.BakimPersonelleri.Add(new BakimPersonelEntity
            {
                BakimId = bakim.Id,
                PersonelId = personel.Id
            });
            sonuc.Eklendi("BakimPersonelleri");
        }

        void ResimEkle(
            BakimPlaniEntity bakim,
            string kaynakId,
            string resimTip,
            string? resimUrl,
            string? driveDosyaId,
            DateTime kayitTarihi)
        {
            resimUrl ??= string.Empty;
            driveDosyaId ??= string.Empty;
            if (string.IsNullOrWhiteSpace(resimUrl) &&
                string.IsNullOrWhiteSpace(driveDosyaId)) return;

            string hamAnahtar = string.Join('|',
                bakim.Id,
                resimTip.Trim().ToUpperInvariant(),
                driveDosyaId.Trim(),
                resimUrl.Trim());
            string anahtar = Convert.ToHexString(
                SHA256.HashData(Encoding.UTF8.GetBytes(hamAnahtar)));

            if (!mevcutDetayAnahtarlari.Add(anahtar)) return;
            _sql.BakimDetaylari.Add(new BakimDetayEntity
            {
                FirestoreId = kaynakId,
                BakimId = bakim.Id,
                ResimTip = resimTip.Trim().ToUpperInvariant(),
                ResimUrl = resimUrl,
                DriveDosyaId = driveDosyaId,
                LegacyKey = anahtar,
                KayitTarihi = kayitTarihi
            });
            sonuc.Eklendi("BakimDetaylari");
        }

        foreach (var kayit in bakimKaynak)
        {
            if (!bakimMap.TryGetValue(kayit.Id, out var bakim)) continue;
            foreach (int personelId in kayit.Veri.PersonelIdleri ?? new List<int>())
                BaglantiEkle(bakim, personelId, kayit.Id);

            // Eski planda doğrudan URL tutulmuşsa kaybolmasın.
            ResimEkle(
                bakim,
                "plan-oncesi-" + kayit.Id,
                "O",
                kayit.Veri.OncesiResimUrl,
                string.Empty,
                kayit.Veri.KayitTarihi);
            ResimEkle(
                bakim,
                "plan-sonrasi-" + kayit.Id,
                "S",
                kayit.Veri.SonrasiResimUrl,
                string.Empty,
                kayit.Veri.KayitTarihi);
        }

        foreach (var kayit in detayKaynak)
        {
            var x = kayit.Veri;
            if (!bakimMap.TryGetValue(x.BakimId, out var bakim))
            {
                sonuc.Eslesmedi("BakimDetaylari", kayit.Id);
                continue;
            }

            BaglantiEkle(bakim, x.PersonelNo, kayit.Id);
            ResimEkle(
                bakim,
                kayit.Id,
                x.ResimTip,
                x.ResimUrl,
                x.DriveDosyaId,
                x.KayitTarihi);
        }

        await _sql.SaveChangesAsync(ct);
    }

    private async Task TeklifleriAktar(
        IReadOnlyList<FirestoreKaydi<Teklif>> kaynak,
        VeriAktarimSonucu sonuc,
        CancellationToken ct)
    {
        var musteriNoMap = (await _sql.Musteriler.ToListAsync(ct))
            .GroupBy(x => x.MusteriNo)
            .ToDictionary(x => x.Key, x => x.First());
        var mevcut = await _sql.Teklifler
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);

        foreach (var kayit in kaynak)
        {
            if (!mevcut.TryGetValue(kayit.Id, out var hedef))
            {
                hedef = new TeklifEntity { FirestoreId = kayit.Id };
                _sql.Teklifler.Add(hedef);
                mevcut[kayit.Id] = hedef;
                sonuc.Eklendi("Teklifler");
            }
            else
            {
                sonuc.Guncellendi("Teklifler");
            }

            var x = kayit.Veri;
            hedef.MusteriId = musteriNoMap.TryGetValue(x.MusteriNo, out var m)
                ? m.Id
                : null;
            hedef.MusteriNo = x.MusteriNo;
            hedef.AdSoyad = x.AdSoyad;
            hedef.Telefon = x.Telefon;
            hedef.TeklifTarihi = x.TeklifTarihi;
            hedef.Aciklama = x.Aciklama;
            hedef.Tutar = Convert.ToDecimal(x.Tutar);
            hedef.DurumKodu = x.DurumKodu;
            hedef.IslemNotu = x.IslemNotu;
            hedef.KayitTarihi = x.KayitTarihi;
            hedef.IslemTarihi = x.IslemTarihi;
        }

        await _sql.SaveChangesAsync(ct);
    }

    private async Task MusteriKullanicilariniAktar(
        IReadOnlyList<FirestoreKaydi<MusteriKullanicisi>> kaynak,
        VeriAktarimSonucu sonuc,
        CancellationToken ct)
    {
        var kullaniciMap = await _sql.Kullanicilar
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);
        var musteriMap = await _sql.Musteriler
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);
        var mevcut = await _sql.MusteriKullanicilari
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);

        foreach (var kayit in kaynak)
        {
            var x = kayit.Veri;
            if (!kullaniciMap.TryGetValue(x.KullaniciId, out var kullanici) ||
                !musteriMap.TryGetValue(x.MusteriId, out var musteri))
            {
                sonuc.Eslesmedi("MusteriKullanicilari", kayit.Id);
                continue;
            }

            if (!mevcut.TryGetValue(kayit.Id, out var hedef))
            {
                hedef = new MusteriKullaniciEntity { FirestoreId = kayit.Id };
                _sql.MusteriKullanicilari.Add(hedef);
                mevcut[kayit.Id] = hedef;
                sonuc.Eklendi("MusteriKullanicilari");
            }
            else
            {
                sonuc.Guncellendi("MusteriKullanicilari");
            }

            hedef.KullaniciId = kullanici.Id;
            hedef.MusteriId = musteri.Id;
        }

        await _sql.SaveChangesAsync(ct);
    }

    private async Task IsleriAktar(
        IReadOnlyList<FirestoreKaydi<Isler>> isKaynak,
        IReadOnlyList<FirestoreKaydi<YapilacakIs>> yapilacakKaynak,
        IReadOnlyList<FirestoreKaydi<YapilmayacakIs>> yapilmayacakKaynak,
        VeriAktarimSonucu sonuc,
        CancellationToken ct)
    {
        var musteriNoMap = (await _sql.Musteriler.ToListAsync(ct))
            .GroupBy(x => x.MusteriNo)
            .ToDictionary(x => x.Key, x => x.First());

        var mevcutIsler = await _sql.Isler
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);
        foreach (var kayit in isKaynak)
        {
            if (!mevcutIsler.TryGetValue(kayit.Id, out var hedef))
            {
                hedef = new IsEntity { FirestoreId = kayit.Id };
                _sql.Isler.Add(hedef);
                mevcutIsler[kayit.Id] = hedef;
                sonuc.Eklendi("Isler");
            }
            else sonuc.Guncellendi("Isler");
            hedef.EklentiNo = kayit.Veri.EklentiNo;
            hedef.Tanim = kayit.Veri.Tanim;
        }

        var mevcutYapilacak = await _sql.YapilacakIsler
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);
        foreach (var kayit in yapilacakKaynak)
        {
            if (!mevcutYapilacak.TryGetValue(kayit.Id, out var hedef))
            {
                hedef = new YapilacakIsEntity { FirestoreId = kayit.Id };
                _sql.YapilacakIsler.Add(hedef);
                mevcutYapilacak[kayit.Id] = hedef;
                sonuc.Eklendi("YapilacakIsler");
            }
            else sonuc.Guncellendi("YapilacakIsler");
            var x = kayit.Veri;
            hedef.MusteriId = musteriNoMap.TryGetValue(x.MusteriNo, out var m) ? m.Id : null;
            hedef.MusteriNo = x.MusteriNo;
            hedef.EklentiNo = x.EklentiNo;
            hedef.Not = x.Not;
            hedef.KayitTarihi = x.KayitTarihi;
        }

        var mevcutYapilmayacak = await _sql.YapilmayacakIsler
            .Where(x => x.FirestoreId != null)
            .ToDictionaryAsync(x => x.FirestoreId!, StringComparer.Ordinal, ct);
        foreach (var kayit in yapilmayacakKaynak)
        {
            if (!mevcutYapilmayacak.TryGetValue(kayit.Id, out var hedef))
            {
                hedef = new YapilmayacakIsEntity { FirestoreId = kayit.Id };
                _sql.YapilmayacakIsler.Add(hedef);
                mevcutYapilmayacak[kayit.Id] = hedef;
                sonuc.Eklendi("YapilmayacakIsler");
            }
            else sonuc.Guncellendi("YapilmayacakIsler");
            var x = kayit.Veri;
            hedef.MusteriId = musteriNoMap.TryGetValue(x.MusteriNo, out var m) ? m.Id : null;
            hedef.MusteriNo = x.MusteriNo;
            hedef.EklentiNo = x.EklentiNo;
            hedef.Not = x.Not;
            hedef.KayitTarihi = x.KayitTarihi;
        }

        await _sql.SaveChangesAsync(ct);
    }

    private async Task<List<FirestoreKaydi<T>>> OkuAsync<T>(
        string koleksiyon,
        CancellationToken ct)
    {
        QuerySnapshot snapshot = await _firestore
            .Collection(koleksiyon)
            .GetSnapshotAsync(ct);

        return snapshot.Documents
            .Where(x => x.Exists)
            .Select(x => new FirestoreKaydi<T>(x.Id, x.ConvertTo<T>()))
            .ToList();
    }

    private sealed record FirestoreKaydi<T>(string Id, T Veri);
}

public sealed class VeriAktarimSonucu
{
    public bool Basarili { get; set; }
    public Dictionary<string, int> Eklenenler { get; set; } = new();
    public Dictionary<string, int> Guncellenenler { get; set; } = new();
    public Dictionary<string, List<string>> EslesmeyenKayitlar { get; set; } = new();
    public Dictionary<string, int> SqlToplamlari { get; set; } = new();

    public void Eklendi(string tablo) => Arttir(Eklenenler, tablo);
    public void Guncellendi(string tablo) => Arttir(Guncellenenler, tablo);

    public void Eslesmedi(string tablo, string kaynakId)
    {
        if (!EslesmeyenKayitlar.TryGetValue(tablo, out var liste))
        {
            liste = new List<string>();
            EslesmeyenKayitlar[tablo] = liste;
        }
        liste.Add(kaynakId);
    }

    private static void Arttir(Dictionary<string, int> sayilar, string tablo)
    {
        sayilar[tablo] = sayilar.GetValueOrDefault(tablo) + 1;
    }
}
