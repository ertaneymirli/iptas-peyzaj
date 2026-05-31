using Google.Cloud.Firestore;

namespace IptasPeyzajApi.Backend.Musteriler.Models;

[FirestoreData]
public class Musteri
{
    public string? Id { get; set; }

    [FirestoreProperty]
    public int MusteriNo { get; set; }

    [FirestoreProperty]
    public string Ad { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Soyad { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Tc { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime DogumTarihi { get; set; }

    [FirestoreProperty]
    public string Cinsiyet { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Telefon { get; set; } = string.Empty;

    [FirestoreProperty]
    public string CaddeSokak { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Mahalle { get; set; } = string.Empty;

    [FirestoreProperty]
    public string No { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Daire { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Sehir { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Adres { get; set; } = string.Empty;

    [FirestoreProperty]
    public string MekanTipi { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime SozlesmeTarihi { get; set; }

    [FirestoreProperty]
    public DateTime GorusmeTarihi { get; set; }

    [FirestoreProperty]
    public DateTime BaslangicTarihi { get; set; }

    [FirestoreProperty]
    public DateTime BitisTarihi { get; set; }

    [FirestoreProperty]
    public DateTime BakimTarihi { get; set; }

    [FirestoreProperty]
    public int PeriyodikBakim { get; set; }

    [FirestoreProperty]
    public string PeriyodikBakimTuru { get; set; } = string.Empty;
    // Gun, Hafta, Ay, Yil

    [FirestoreProperty]
    public List<DateTime> BakimTarihleri { get; set; } = new();

    [FirestoreProperty]
    public string Aciklama { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
    [FirestoreProperty]
    public string BelirliGunler { get; set; } = string.Empty;
    [FirestoreProperty]
    public string DurumKodu { get; set; } = "A"; // A: Aktif, P: Pasif
}