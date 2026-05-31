using Google.Cloud.Firestore;

namespace IptasPeyzajApi.Backend.Models;

[FirestoreData]
public class Isler
{
    public string? Id { get; set; }

    [FirestoreProperty]
    public int EklentiNo { get; set; }

    [FirestoreProperty]
    public string Tanim { get; set; } = string.Empty;
}