namespace IptasPeyzajApi.Backend.BakimPlanlari.Models;

public class BakimDetay
{
    public int Id { get; set; }
    public int BakimId { get; set; }
    public int PersonelNo { get; set; }
    public string ResimTip { get; set; } = string.Empty;
    public string ResimUrl { get; set; } = string.Empty;
    public string DriveDosyaId { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
    public string Ad { get; set; } = "";
    public string Soyad { get; set; } = "";
    public string AdSoyad { get; set; } = "";
}
