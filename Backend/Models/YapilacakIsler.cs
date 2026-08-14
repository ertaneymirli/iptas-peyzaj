namespace IptasPeyzajApi.Backend.Models;

public class YapilacakIs
{
    public int Id { get; set; }

    public int MusteriNo { get; set; }  // Musteriler tablosundan

    public int EklentiNo { get; set; }  // Eklentiler tablosundan

    public string Not { get; set; } = string.Empty;

    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
}
