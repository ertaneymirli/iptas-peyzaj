namespace IptasPeyzajApi.Backend.kullanici.Model;

public class Kullanici
{
    public int Id { get; set; }
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
    public string? Sifre { get; set; }
    public string Rol { get; set; } = "2";
    // Admin=1, Personel=2

    public bool AktifMi { get; set; } = true;
    public DateTime? KayitTarihi { get; set; } = DateTime.UtcNow;
}
