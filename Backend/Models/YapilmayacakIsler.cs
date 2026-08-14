namespace IptasPeyzajApi.Backend.Models;

public class YapilmayacakIs
{
    public int Id { get; set; }

    public int MusteriNo { get; set; }

    public int EklentiNo { get; set; }

    public string Not { get; set; } = string.Empty;

    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
}
