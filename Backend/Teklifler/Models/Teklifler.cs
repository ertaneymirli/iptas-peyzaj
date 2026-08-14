namespace IptasPeyzajApi.Backend.Teklifler.Models;

public class Teklif
{
    public int Id { get; set; }
    public int? MusteriId { get; set; }
    public int MusteriNo { get; set; }
    public string AdSoyad { get; set; } = string.Empty;
    public string Telefon { get; set; } = string.Empty;
    public DateTime TeklifTarihi { get; set; }
    public string Aciklama { get; set; } = string.Empty;
    public decimal Tutar { get; set; }
    public string DurumKodu { get; set; } = "B";
    // B: Bekliyor, O: Onaylandı, R: Reddedildi

    public string IslemNotu { get; set; } = string.Empty;
    public DateTime KayitTarihi { get; set; }
    public DateTime? IslemTarihi { get; set; }
}
