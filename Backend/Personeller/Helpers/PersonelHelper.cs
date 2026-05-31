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
                liste.Add(personel);
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
}