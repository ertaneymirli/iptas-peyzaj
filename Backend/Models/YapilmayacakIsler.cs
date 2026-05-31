using Google.Cloud.Firestore;

namespace IptasPeyzajApi.Backend.Models;

[FirestoreData]
public class YapilmayacakIs
{
    public string? Id { get; set; }

    [FirestoreProperty]
    public int MusteriNo { get; set; }

    [FirestoreProperty]
    public int EklentiNo { get; set; }

    [FirestoreProperty]
    public string Not { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
}