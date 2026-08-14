namespace IptasPeyzajApi.Backend.Musteriler.Models;

public class Musteri
{
    public int Id { get; set; }
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
    // Musteriler tablosunda tutulmaz; sıradaki bekleyen BakimPlanlari
    // kaydından hesaplanır ve yeni plan oluşturulurken başlangıç tarihidir.
    public DateTime BakimTarihi { get; set; }
    public int PeriyodikBakim { get; set; }
    public string PeriyodikBakimTuru { get; set; } = string.Empty;
    public string Aciklama { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
    public string BelirliGunler { get; set; } = string.Empty;
    public string DurumKodu { get; set; } = "A"; // A: Aktif, P: Pasif
}
