using Google.Cloud.Firestore;
using IptasPeyzajApi.Backend.Teklifler.Models;

namespace IptasPeyzajApi.Backend.Teklifler.Helpers;

public class TeklifHelper
{
    private readonly FirestoreDb _db;
    private const string CollectionName = "teklifler";

    public TeklifHelper(FirestoreDb db)
    {
        _db = db;
    }

    public async Task<List<Teklif>> TumTeklifleriGetir()
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .OrderByDescending("KayitTarihi")
            .GetSnapshotAsync();

        List<Teklif> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                Teklif teklif = doc.ConvertTo<Teklif>();

                // PASİFLERİ GÖSTERME
                if (teklif.DurumKodu == "P")
                    continue;

                teklif.Id = doc.Id;

                liste.Add(teklif);
            }
        }

        return liste;
    }

    public async Task<Teklif?> TeklifGetir(string id)
    {
        DocumentSnapshot doc = await _db.Collection(CollectionName)
            .Document(id)
            .GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        Teklif teklif = doc.ConvertTo<Teklif>();
        teklif.Id = doc.Id;

        return teklif;
    }

    public async Task<Teklif> TeklifEkle(Teklif teklif)
    {
        teklif.KayitTarihi = DateTime.UtcNow;
        teklif.DurumKodu = "B";

        if (teklif.TeklifTarihi.Kind != DateTimeKind.Utc)
        {
            teklif.TeklifTarihi = DateTime.SpecifyKind(
                teklif.TeklifTarihi,
                DateTimeKind.Utc
            );
        }

        DocumentReference docRef =
            await _db.Collection(CollectionName).AddAsync(teklif);

        teklif.Id = docRef.Id;

        return teklif;
    }

    public async Task<Teklif?> DurumGuncelle(string id, string durumKodu, string islemNotu)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        await docRef.UpdateAsync(new Dictionary<string, object>
        {
            { "DurumKodu", durumKodu },
            { "IslemNotu", islemNotu ?? string.Empty },
            { "IslemTarihi", DateTime.UtcNow }
        });

        return await TeklifGetir(id);
    }

    public async Task<Teklif?> TeklifSil(string id)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        await docRef.UpdateAsync(new Dictionary<string, object>
    {
        { "DurumKodu", "P" },
        { "IslemTarihi", DateTime.UtcNow },
        { "IslemNotu", "Teklif pasife alındı." }
    });

        return await TeklifGetir(id);
    }
    public async Task<Teklif?> TeklifGuncelle(string id, Teklif teklif)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        await docRef.UpdateAsync(new Dictionary<string, object>
    {
        { "MusteriNo", teklif.MusteriNo },
        { "AdSoyad", teklif.AdSoyad ?? string.Empty },
        { "Telefon", teklif.Telefon ?? string.Empty },
        { "TeklifTarihi", DateTime.SpecifyKind(teklif.TeklifTarihi, DateTimeKind.Utc) },
        { "Tutar", teklif.Tutar },
        { "Aciklama", teklif.Aciklama ?? string.Empty }
    });

        return await TeklifGetir(id);
    }
    public async Task<List<Teklif>> PasifTeklifleriGetir()
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .WhereEqualTo("DurumKodu", "P")
            .GetSnapshotAsync();

        List<Teklif> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                Teklif teklif = doc.ConvertTo<Teklif>();

                teklif.Id = doc.Id;

                liste.Add(teklif);
            }
        }

        return liste;
    }
}