using Google.Cloud.Firestore;

namespace IptasPeyzajApi.Backend.BakimPlanlari.Models;

[FirestoreData]
public class BakimDetay
{
    public string? Id { get; set; }

    [FirestoreProperty]
    public string BakimId { get; set; } = string.Empty;

    [FirestoreProperty]
    public int PersonelNo { get; set; }

    [FirestoreProperty]
    public string ResimTip { get; set; } = string.Empty;

    [FirestoreProperty]
    public string ResimUrl { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
    [FirestoreProperty]
    public string Ad { get; set; } = "";
    [FirestoreProperty]
    public string Soyad { get; set; } = "";
    public string AdSoyad { get; set; } = "";
}