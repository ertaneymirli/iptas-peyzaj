using Google.Cloud.Firestore;

[FirestoreData]
public class Personel
{
    public string? DocId { get; set; }

    [FirestoreProperty("Id")]
    public int Id { get; set; }

    [FirestoreProperty("PersonelNo")]
    public int PersonelNo { get; set; }

    [FirestoreProperty("Ad")]
    public string Ad { get; set; } = string.Empty;

    [FirestoreProperty("Soyad")]
    public string Soyad { get; set; } = string.Empty;

    public string AdSoyad => $"{Ad} {Soyad}";

    [FirestoreProperty]
    public string Telefon { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Gorev { get; set; } = string.Empty;

    [FirestoreProperty]
    public string DurumKodu { get; set; } = "A";

    [FirestoreProperty]
    public DateTime KayitTarihi { get; set; }
}