namespace IptasPeyzajApi.Backend.Data.Entities;

public abstract class SqlEntity
{
    public int Id { get; set; }
    public string? FirestoreId { get; set; }
}

public sealed class MusteriEntity : SqlEntity
{
    public int MusteriNo { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Tc { get; set; } = string.Empty;
    public DateTime DogumTarihi { get; set; }
    public string Cinsiyet { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string CaddeSokak { get; set; } = string.Empty;
    public string Mahalle { get; set; } = string.Empty;
    public string No { get; set; } = string.Empty;
    public string Daire { get; set; } = string.Empty;
    public string Sehir { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public string MekanTipi { get; set; } = string.Empty;
    public DateTime SozlesmeTarihi { get; set; }
    public DateTime GorusmeTarihi { get; set; }
    public DateTime BaslangicTarihi { get; set; }
    public DateTime BitisTarihi { get; set; }
    public int PeriyodikBakim { get; set; }
    public string PeriyodikBakimTuru { get; set; } = string.Empty;
    public string BelirliGunler { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }
    public string DurumKodu { get; set; } = "A";

    public ICollection<BakimPlaniEntity> BakimPlanlari { get; set; } =
        new List<BakimPlaniEntity>();
    public ICollection<MusteriKullaniciEntity> KullaniciBaglantilari { get; set; } =
        new List<MusteriKullaniciEntity>();
}

public sealed class BakimPlaniEntity : SqlEntity
{
    public int MusteriId { get; set; }
    public int MusteriNo { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public DateTime BakimTarihi { get; set; }
    public string DurumKodu { get; set; } = "B";
    public string Aciklama { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }
    public DateTime? IslemTarihi { get; set; }
    public string IslemNotu { get; set; } = string.Empty;

    public MusteriEntity Musteri { get; set; } = null!;
    public ICollection<BakimDetayEntity> Detaylar { get; set; } =
        new List<BakimDetayEntity>();
    public ICollection<BakimPersonelEntity> Personeller { get; set; } =
        new List<BakimPersonelEntity>();
}

public sealed class BakimDetayEntity : SqlEntity
{
    public int BakimId { get; set; }
    public string ResimTip { get; set; } = string.Empty;
    public string ResimUrl { get; set; } = string.Empty;
    public string DriveDosyaId { get; set; } = string.Empty;
    public string LegacyKey { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }

    public BakimPlaniEntity Bakim { get; set; } = null!;
}

public sealed class BakimPersonelEntity
{
    public int Id { get; set; }
    public int BakimId { get; set; }
    public int PersonelId { get; set; }

    public BakimPlaniEntity Bakim { get; set; } = null!;
    public PersonelEntity Personel { get; set; } = null!;
}

public sealed class KullaniciEntity : SqlEntity
{
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public DateTime? DogumTarihi { get; set; }
    public string Tc { get; set; } = string.Empty;
    public string TelefonNo { get; set; } = string.Empty;
    public string CepTelefonNo { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string SifreHash { get; set; } = string.Empty;
    public string Rol { get; set; } = "2";
    public bool AktifMi { get; set; } = true;
    public DateTime? KayitTarihi { get; set; }

    public ICollection<MusteriKullaniciEntity> MusteriBaglantilari { get; set; } =
        new List<MusteriKullaniciEntity>();
}

public sealed class PersonelEntity : SqlEntity
{
    public int EskiPersonelId { get; set; }
    public int PersonelNo { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public string Gorev { get; set; } = string.Empty;
    public string DurumKodu { get; set; } = "A";
    public DateTime KayitTarihi { get; set; }

    public ICollection<BakimPersonelEntity> Bakimlar { get; set; } =
        new List<BakimPersonelEntity>();
}

public sealed class TeklifEntity : SqlEntity
{
    public int? MusteriId { get; set; }
    public int MusteriNo { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public DateTime TeklifTarihi { get; set; }
    public string Aciklama { get; set; } = string.Empty;
    public decimal Tutar { get; set; }
    public string DurumKodu { get; set; } = "B";
    public string IslemNotu { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }
    public DateTime? IslemTarihi { get; set; }

    public MusteriEntity? Musteri { get; set; }
}

public sealed class MusteriKullaniciEntity : SqlEntity
{
    public int KullaniciId { get; set; }
    public int MusteriId { get; set; }

    public KullaniciEntity Kullanici { get; set; } = null!;
    public MusteriEntity Musteri { get; set; } = null!;
}

public sealed class IsEntity : SqlEntity
{
    public int EklentiNo { get; set; }
    public string Tanim { get; set; } = string.Empty;
}

public sealed class YapilacakIsEntity : SqlEntity
{
    public int? MusteriId { get; set; }
    public int MusteriNo { get; set; }
    public int EklentiNo { get; set; }
    public string Not { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }

    public MusteriEntity? Musteri { get; set; }
}

public sealed class YapilmayacakIsEntity : SqlEntity
{
    public int? MusteriId { get; set; }
    public int MusteriNo { get; set; }
    public int EklentiNo { get; set; }
    public string Not { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }

    public MusteriEntity? Musteri { get; set; }
}
