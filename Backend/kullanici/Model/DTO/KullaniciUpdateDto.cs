namespace IptasPeyzajApi.Backend.kullanici.Model.DTO;

public class KullaniciUpdateDto
{
    public string KullaniciAdi { get; set; } = string.Empty;
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;
    public DateTime DogumTarihi { get; set; }
    public string Tc { get; set; } = string.Empty;
    public string TelefonNo { get; set; } = string.Empty;
    public string CepTelefonNo { get; set; } = string.Empty;
    public string Adres { get; set; } = string.Empty;
    public string Mail { get; set; } = string.Empty;
    public string? Sifre { get; set; }
    public string Rol { get; set; } = "Personel";
    public bool AktifMi { get; set; } = true;
}
