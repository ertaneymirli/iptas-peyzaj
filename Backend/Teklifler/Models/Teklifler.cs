using Google.Cloud.Firestore;

namespace IptasPeyzajApi.Backend.Teklifler.Models;

[FirestoreData]
public class Teklif
{
    public string? Id { get; set; }

    [FirestoreProperty]
    public int MusteriNo { get; set; }

    [FirestoreProperty]
    public string AdSoyad { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Telefon { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime TeklifTarihi { get; set; }

    [FirestoreProperty]
    public string Aciklama { get; set; } = string.Empty;

    [FirestoreProperty]
    public double Tutar { get; set; }

    [FirestoreProperty]
    public string DurumKodu { get; set; } = "B";
    // B: Bekliyor, O: Onaylandı, R: Reddedildi

    [FirestoreProperty]
    public string IslemNotu { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime KayitTarihi { get; set; }

    [FirestoreProperty]
    public DateTime? IslemTarihi { get; set; }
}