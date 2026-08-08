using Google.Cloud.Firestore;

namespace IptasPeyzajApi.Backend.MusteriKullanicilari.Models;

[FirestoreData]
public class MusteriKullanicisi
{
    public string? Id { get; set; }

    [FirestoreProperty]
    public string KullaniciId { get; set; } = string.Empty;

    [FirestoreProperty]
    public string MusteriId { get; set; } = string.Empty;
}