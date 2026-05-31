namespace IptasPeyzajApi.Backend.Helpers;

public static class BakimHelper
{
    public static List<DateTime> BakimTarihleriOlustur(
        DateTime ilkBakimTarihi,
        DateTime bitisTarihi,
        int periyodikBakim,
        string periyodikBakimTuru)
    {
        List<DateTime> tarihler = new();

        if (periyodikBakim <= 0)
            return tarihler;

        DateTime tarih = ilkBakimTarihi;

        while (tarih <= bitisTarihi)
        {
            tarihler.Add(tarih);

            tarih = periyodikBakimTuru.ToLower() switch
            {
                "gün" or "gun" => tarih.AddDays(periyodikBakim),
                "hafta" => tarih.AddDays(periyodikBakim * 7),
                "ay" => tarih.AddMonths(periyodikBakim),
                "yıl" or "yil" => tarih.AddYears(periyodikBakim),
                _ => tarih.AddMonths(periyodikBakim)
            };
        }

        return tarihler;
    }
}