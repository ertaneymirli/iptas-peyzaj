using Google.Cloud.Firestore;
using IptasPeyzajApi.Backend.MusteriKullanicilari.Models;

namespace IptasPeyzajApi.Backend.MusteriKullanicilari.Helpers;

public class MusteriKullanicisiHelper
{
    private const string KoleksiyonAdi =
        "musteriKullanicilari";

    private readonly FirestoreDb _firestoreDb;

    public MusteriKullanicisiHelper(
        FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public async Task<List<MusteriKullanicisi>>
        TumBaglantilariGetir()
    {
        QuerySnapshot snapshot = await _firestoreDb
            .Collection(KoleksiyonAdi)
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(BelgedenModeleCevir)
            .ToList();
    }

    public async Task<MusteriKullanicisi?>
        BaglantiGetir(string id)
    {
        DocumentSnapshot snapshot = await _firestoreDb
            .Collection(KoleksiyonAdi)
            .Document(id)
            .GetSnapshotAsync();

        if (!snapshot.Exists)
            return null;

        return BelgedenModeleCevir(snapshot);
    }

    public async Task<List<MusteriKullanicisi>>
        KullaniciyaGoreGetir(string kullaniciId)
    {
        QuerySnapshot snapshot = await _firestoreDb
            .Collection(KoleksiyonAdi)
            .WhereEqualTo(
                nameof(MusteriKullanicisi.KullaniciId),
                kullaniciId)
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(BelgedenModeleCevir)
            .ToList();
    }

    public async Task<List<MusteriKullanicisi>>
        MusteriyeGoreGetir(string musteriId)
    {
        QuerySnapshot snapshot = await _firestoreDb
            .Collection(KoleksiyonAdi)
            .WhereEqualTo(
                nameof(MusteriKullanicisi.MusteriId),
                musteriId)
            .GetSnapshotAsync();

        return snapshot.Documents
            .Select(BelgedenModeleCevir)
            .ToList();
    }

    public async Task<MusteriKullanicisi>
        BaglantiEkle(MusteriKullanicisi model)
    {
        if (string.IsNullOrWhiteSpace(model.KullaniciId))
            throw new ArgumentException(
                "Kullanıcı ID boş olamaz.");

        if (string.IsNullOrWhiteSpace(model.MusteriId))
            throw new ArgumentException(
                "Müşteri ID boş olamaz.");

        string belgeId =
            $"{model.KullaniciId}_{model.MusteriId}";

        DocumentReference belge = _firestoreDb
            .Collection(KoleksiyonAdi)
            .Document(belgeId);

        DocumentSnapshot mevcut =
            await belge.GetSnapshotAsync();

        if (mevcut.Exists)
            throw new InvalidOperationException(
                "Bu kullanıcı ile müşteri zaten eşleştirilmiş.");

        model.Id = belgeId;

        await belge.SetAsync(model);

        return model;
    }

    public async Task<bool> BaglantiSil(string id)
    {
        DocumentReference belge = _firestoreDb
            .Collection(KoleksiyonAdi)
            .Document(id);

        DocumentSnapshot snapshot =
            await belge.GetSnapshotAsync();

        if (!snapshot.Exists)
            return false;

        await belge.DeleteAsync();

        return true;
    }

    public async Task<bool> KullaniciMusteriBaglantisiSil(
        string kullaniciId,
        string musteriId)
    {
        string belgeId =
            $"{kullaniciId}_{musteriId}";

        return await BaglantiSil(belgeId);
    }

    private static MusteriKullanicisi
        BelgedenModeleCevir(DocumentSnapshot belge)
    {
        MusteriKullanicisi model =
            belge.ConvertTo<MusteriKullanicisi>();

        model.Id = belge.Id;

        return model;
    }
}