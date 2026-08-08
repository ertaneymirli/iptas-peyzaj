using System.Security.Claims;
using IptasPeyzajApi.Backend.BakimPlanlari.Helpers;
using IptasPeyzajApi.Backend.BakimPlanlari.Models;
using IptasPeyzajApi.Backend.Musteriler.Helpers;
using IptasPeyzajApi.Backend.MusteriKullanicilari.Helpers;
using IptasPeyzajApi.Backend.Personeller.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IptasPeyzajApi.Backend.BakimPlanlari.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class BakimPlanlariController : ControllerBase
{
    private readonly MusteriHelper _musteriHelper;
    private readonly BakimPlaniHelper _helper;
    private readonly PersonelHelper _personelHelper;
    private readonly MusteriKullanicisiHelper
        _musteriKullanicisiHelper;

    public BakimPlanlariController(
        BakimPlaniHelper helper,
        MusteriHelper musteriHelper,
        PersonelHelper personelHelper,
        MusteriKullanicisiHelper musteriKullanicisiHelper)
    {
        _helper = helper;
        _musteriHelper = musteriHelper;
        _personelHelper = personelHelper;
        _musteriKullanicisiHelper =
            musteriKullanicisiHelper;
    }

    private bool AdminMi()
    {
        return User.IsInRole("1");
    }

    private string? KullaniciIdGetir()
    {
        return User.FindFirstValue("id");
    }

    private async Task<HashSet<string>>
        YetkiliMusteriIdleriniGetir()
    {
        string? kullaniciId =
            KullaniciIdGetir();

        if (string.IsNullOrWhiteSpace(kullaniciId))
        {
            return new HashSet<string>();
        }

        var baglantilar =
            await _musteriKullanicisiHelper
                .KullaniciyaGoreGetir(kullaniciId);

        return baglantilar
            .Where(x =>
                !string.IsNullOrWhiteSpace(
                    x.MusteriId
                )
            )
            .Select(x => x.MusteriId)
            .ToHashSet(StringComparer.Ordinal);
    }

    private async Task<List<BakimPlani>>
        KullaniciyaGoreFiltrele(
            List<BakimPlani> bakimlar)
    {
        if (AdminMi())
        {
            return bakimlar;
        }

        HashSet<string> musteriIdleri =
            await YetkiliMusteriIdleriniGetir();

        return bakimlar
            .Where(x =>
                !string.IsNullOrWhiteSpace(
                    x.MusteriId
                ) &&
                musteriIdleri.Contains(
                    x.MusteriId
                )
            )
            .ToList();
    }

    private async Task<bool>
        BakimaErisimVarMi(BakimPlani bakim)
    {
        if (AdminMi())
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(
            bakim.MusteriId))
        {
            return false;
        }

        HashSet<string> musteriIdleri =
            await YetkiliMusteriIdleriniGetir();

        return musteriIdleri.Contains(
            bakim.MusteriId
        );
    }

    private async Task MusteriBilgileriniDoldur(
        List<BakimPlani> bakimlar)
    {
        var musteriler =
            await _musteriHelper
                .TumMusterileriGetir();

        var musteriIdMap = musteriler
            .Where(x =>
                !string.IsNullOrWhiteSpace(x.Id))
            .ToDictionary(
                x => x.Id!,
                x => x,
                StringComparer.Ordinal
            );

        foreach (BakimPlani bakim in bakimlar)
        {
            if (
                string.IsNullOrWhiteSpace(
                    bakim.MusteriId
                )
            )
            {
                continue;
            }

            if (
                musteriIdMap.TryGetValue(
                    bakim.MusteriId,
                    out var musteri
                )
            )
            {
                bakim.MusteriNo =
                    musteri.MusteriNo;

                bakim.AdSoyad =
                    $"{musteri.Ad} {musteri.Soyad}";

                bakim.Telefon =
                    musteri.Telefon;
            }
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        if (
            !AdminMi() &&
            string.IsNullOrWhiteSpace(
                KullaniciIdGetir()
            )
        )
        {
            return Unauthorized(
                "Token içerisinde kullanıcı ID bulunamadı."
            );
        }

        List<BakimPlani> bakimlar =
            await _helper.TumBakimlariGetir();

        bakimlar =
            await KullaniciyaGoreFiltrele(
                bakimlar
            );

        await MusteriBilgileriniDoldur(
            bakimlar
        );

        return Ok(bakimlar);
    }

    [HttpGet("durum/{durumKodu}")]
    public async Task<IActionResult> GetByDurum(
        string durumKodu)
    {
        if (
            !AdminMi() &&
            string.IsNullOrWhiteSpace(
                KullaniciIdGetir()
            )
        )
        {
            return Unauthorized(
                "Token içerisinde kullanıcı ID bulunamadı."
            );
        }

        List<BakimPlani> liste =
            await _helper.DurumaGoreGetir(
                durumKodu
            );

        liste =
            await KullaniciyaGoreFiltrele(
                liste
            );

        await MusteriBilgileriniDoldur(
            liste
        );

        return Ok(liste);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(
        string id)
    {
        BakimPlani? bakim =
            await _helper.BakimGetir(id);

        if (bakim == null)
        {
            return NotFound(
                "Bakım planı bulunamadı."
            );
        }

        if (!await BakimaErisimVarMi(bakim))
        {
            return Forbid();
        }

        await MusteriBilgileriniDoldur(
            new List<BakimPlani>
            {
                bakim
            }
        );

        return Ok(bakim);
    }

    [Authorize(Roles = "1")]
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] BakimPlani bakim)
    {
        var sonuc =
            await _helper.BakimEkle(bakim);

        return Ok(sonuc);
    }

    [Authorize(Roles = "1")]
    [HttpPut("{id}/durum")]
    public async Task<IActionResult> DurumGuncelle(
        string id,
        [FromBody] BakimDurumDto dto)
    {
        var sonuc =
            await _helper.DurumGuncelle(
                id,
                dto.DurumKodu,
                dto.IslemNotu
            );

        if (sonuc == null)
        {
            return NotFound(
                "Bakım planı bulunamadı."
            );
        }

        return Ok(sonuc);
    }

    [Authorize(Roles = "1")]
    [HttpPut("{id}/ertele")]
    public async Task<IActionResult> Ertele(
        string id,
        [FromBody] BakimErteleDto dto)
    {
        var sonuc = await _helper.Ertele(
            id,
            dto.YeniTarih,
            dto.IslemNotu
        );

        if (sonuc == null)
        {
            return NotFound(
                "Bakım planı bulunamadı."
            );
        }

        return Ok(sonuc);
    }

    [Authorize(Roles = "1")]
    [HttpPut("{id}/tamamla")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Tamamla(
      string id,
      [FromForm] BakimTamamlaDto dto)
    {
        var personelMetinleri =
            (dto.PersonelIdleri ?? "")
                .Split(
                    ",",
                    StringSplitOptions.RemoveEmptyEntries
                )
                .Select(x => x.Trim())
                .ToList();

        if (personelMetinleri.Any(
            x => !int.TryParse(x, out _)))
        {
            return BadRequest(
                $"Geçersiz personel ID: {dto.PersonelIdleri}"
            );
        }

        List<int> personelIdleri =
            personelMetinleri
                .Select(int.Parse)
                .ToList();

        var sonuc =
            await _helper.BakimTamamla(
                id,
                personelIdleri,
                dto.IslemNotu ?? "",
                dto.OncesiResim,
                dto.SonrasiResim
            );

        if (sonuc == null)
        {
            return NotFound(
                "Bakım planı bulunamadı."
            );
        }

        return Ok(sonuc);
    }

    [HttpGet("{id}/detaylar")]
    public async Task<IActionResult> Detaylar(
        string id)
    {
        BakimPlani? bakim =
            await _helper.BakimGetir(id);

        if (bakim == null)
        {
            return NotFound(
                "Bakım planı bulunamadı."
            );
        }

        if (!await BakimaErisimVarMi(bakim))
        {
            return Forbid();
        }

        var detaylar =
            await _helper
                .BakimDetaylariGetir(id);

        var personeller =
            await _personelHelper
                .TumPersonelleriGetir();

        foreach (var detay in detaylar)
        {
            var personel =
                personeller.FirstOrDefault(
                    x =>
                        x.Id ==
                        detay.PersonelNo
                );

            if (personel != null)
            {
                detay.AdSoyad =
                    $"{personel.Ad} {personel.Soyad}";
            }
        }

        return Ok(detaylar);
    }
}

public class BakimDurumDto
{
    public string DurumKodu { get; set; } =
        "B";

    public string IslemNotu { get; set; } =
        string.Empty;
}

public class BakimErteleDto
{
    public DateTime YeniTarih { get; set; }

    public string IslemNotu { get; set; } =
        string.Empty;
}

public class BakimTamamlaDto
{
    public string? PersonelIdleri { get; set; }

    public string? IslemNotu { get; set; }

    public IFormFile? OncesiResim { get; set; }

    public IFormFile? SonrasiResim { get; set; }
}