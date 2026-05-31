using Google.Cloud.Firestore;


namespace IptasPeyzajApi.Backend.Personeller.Helpers;

public class PersonelHelper
{
    private readonly FirestoreDb _db;
    private const string CollectionName = "Personeller";

    public PersonelHelper(FirestoreDb db)
    {
        _db = db;
    }

    public async Task<List<Personel>> TumPersonelleriGetir()
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .OrderBy("PersonelNo")
            .GetSnapshotAsync();

        List<Personel> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                Personel personel = doc.ConvertTo<Personel>();
                personel.DocId = doc.Id;
                if (personel.DurumKodu != "P")
                {
                    liste.Add(personel);
                }
            }
        }

        return liste;
    }

    public async Task<Personel?> PersonelGetir(string docId)
    {
        DocumentSnapshot doc = await _db.Collection(CollectionName)
            .Document(docId)
            .GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        Personel personel = doc.ConvertTo<Personel>();
        personel.DocId = doc.Id;

        return personel;
    }
    public async Task<Personel> PersonelEkle(Personel personel)
    {
        personel.KayitTarihi = DateTime.UtcNow;
        personel.DurumKodu = "A";

        DocumentReference addedDoc =
            await _db.Collection(CollectionName).AddAsync(personel);

        personel.DocId = addedDoc.Id;

        return personel;
    }

    public async Task<Personel?> PersonelGuncelle(string docId, Personel personel)
    {
        DocumentReference docRef =
            _db.Collection(CollectionName).Document(docId);

        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        await docRef.UpdateAsync(new Dictionary<string, object>
    {
        { "Id", personel.Id },
        { "PersonelNo", personel.PersonelNo },
        { "Ad", personel.Ad ?? string.Empty },
        { "Soyad", personel.Soyad ?? string.Empty },
        { "Telefon", personel.Telefon ?? string.Empty },
        { "Gorev", personel.Gorev ?? string.Empty }
    });

        return await PersonelGetir(docId);
    }

    public async Task<Personel?> DurumGuncelle(string docId, string durumKodu)
    {
        DocumentReference docRef =
            _db.Collection(CollectionName).Document(docId);

        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return null;

        await docRef.UpdateAsync(new Dictionary<string, object>
    {
        { "DurumKodu", durumKodu }
    });

        return await PersonelGetir(docId);
    }

    public async Task<List<Personel>> DurumaGoreGetir(string durumKodu)
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .WhereEqualTo("DurumKodu", durumKodu)
            .GetSnapshotAsync();

        List<Personel> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            if (doc.Exists)
            {
                Personel personel = doc.ConvertTo<Personel>();
                personel.DocId = doc.Id;
                liste.Add(personel);
            }
        }

        return liste.OrderBy(x => x.PersonelNo).ToList();
    }
}