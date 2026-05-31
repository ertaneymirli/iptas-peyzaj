using Google.Cloud.Firestore;

namespace IptasPeyzajApi.Backend.Models;

[FirestoreData]
public class YapilacakIs
{
    public string? Id { get; set; }

    [FirestoreProperty]
    public int MusteriNo { get; set; }  // Musteriler tablosundan

    [FirestoreProperty]
    public int EklentiNo { get; set; }  // Eklentiler tablosundan

    [FirestoreProperty]
    public string Not { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
}