using Google.Cloud.Firestore;

namespace IptasPeyzajApi.Backend.BakimPlanlari.Models;

[FirestoreData]
public class BakimPlani
{
    public string? Id { get; set; }

    [FirestoreProperty]
    public string MusteriId { get; set; } = string.Empty;

    [FirestoreProperty]
    public int MusteriNo { get; set; }

    [FirestoreProperty]
    public string AdSoyad { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Telefon { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime BakimTarihi { get; set; }

    [FirestoreProperty]
    public string DurumKodu { get; set; } = "B";
    // B: Bekliyor, T: Tamamlandı, I: İptal, E: Ertelendi

    [FirestoreProperty]
    public string Aciklama { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;

    [FirestoreProperty]
    public DateTime? IslemTarihi { get; set; }

    [FirestoreProperty]
    public string IslemNotu { get; set; } = string.Empty;
    [FirestoreProperty]
    public List<int> PersonelIdleri { get; set; } = new();

    [FirestoreProperty]
    public string OncesiResimUrl { get; set; } = string.Empty;

    [FirestoreProperty]
    public string SonrasiResimUrl { get; set; } = string.Empty;
}