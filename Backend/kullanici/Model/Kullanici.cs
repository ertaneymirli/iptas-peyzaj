using Google.Cloud.Firestore;

namespace IptasPeyzajApi.Backend.kullanici.Model;

[FirestoreData]
public class Kullanici
{
    public string? Id { get; set; }

    [FirestoreProperty]
    public string KullaniciAdi { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Ad { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Soyad { get; set; } = string.Empty;

    [FirestoreProperty]
    public DateTime DogumTarihi { get; set; }

    [FirestoreProperty]
    public string Tc { get; set; } = string.Empty;

    [FirestoreProperty]
    public string TelefonNo { get; set; } = string.Empty;

    [FirestoreProperty]
    public string CepTelefonNo { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Adres { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Mail { get; set; } = string.Empty;

    // Şifre düz yazı tutulmaz
    [FirestoreProperty]
    public string SifreHash { get; set; } = string.Empty;

    [FirestoreProperty]
    public string Rol { get; set; } = "Personel";
    // Admin, Personel, Yonetici gibi

    [FirestoreProperty]
    public bool AktifMi { get; set; } = true;

    [FirestoreProperty]
    public DateTime KayitTarihi { get; set; } = DateTime.UtcNow;
}