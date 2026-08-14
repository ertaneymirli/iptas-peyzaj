namespace IptasPeyzajApi.Backend.BakimPlanlari.Models;

public class BakimPlani
{
    public int Id { get; set; }
    public int MusteriId { get; set; }
    public int MusteriNo { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public DateTime BakimTarihi { get; set; }
    public string DurumKodu { get; set; } = "B";
    // B: Bekliyor, T: Tamamlandı, I: İptal, E: Ertelendi

    public string Aciklama { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
    public DateTime? IslemTarihi { get; set; }
    public string IslemNotu { get; set; } = string.Empty;
    public List<int> PersonelIdleri { get; set; } = new();
    public string OncesiResimUrl { get; set; } = string.Empty;
    public string SonrasiResimUrl { get; set; } = string.Empty;
}
