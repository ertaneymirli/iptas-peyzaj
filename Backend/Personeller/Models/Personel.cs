public class Personel
{
    public int Id { get; set; }
    public int PersonelNo { get; set; }
    public string Ad { get; set; } = string.Empty;
    public string Soyad { get; set; } = string.Empty;

    public string AdSoyad => $"{Ad} {Soyad}";

    public string Telefon { get; set; } = string.Empty;
    public string Gorev { get; set; } = string.Empty;
    public string DurumKodu { get; set; } = "A";
    public DateTime KayitTarihi { get; set; }
}
