using Microsoft.AspNetCore.Authorization;
using IptasPeyzajApi.Backend.BakimPlanlari.Helpers;
using IptasPeyzajApi.Backend.BakimPlanlari.Models;
using IptasPeyzajApi.Backend.Musteriler.Helpers;
using IptasPeyzajApi.Backend.Personeller.Helpers;
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

    public BakimPlanlariController(
    BakimPlaniHelper helper,
    MusteriHelper musteriHelper,
        PersonelHelper personelHelper)
    {
        _helper = helper;
        _musteriHelper = musteriHelper;
        _personelHelper = personelHelper;
    }


    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var bakimlar = await _helper.TumBakimlariGetir();
        var musteriler = await _musteriHelper.TumMusterileriGetir();

        var musteriMap = musteriler.ToDictionary(x => x.MusteriNo);

        foreach (var bakim in bakimlar)
        {
            if (musteriMap.TryGetValue(bakim.MusteriNo, out var musteri))
            {
                bakim.AdSoyad = $"{musteri.Ad} {musteri.Soyad}";
                bakim.Telefon = musteri.Telefon;
            }
        }

        return Ok(bakimlar);
    }

    [HttpGet("durum/{durumKodu}")]
    public async Task<IActionResult> GetByDurum(string durumKodu)
    {
        var liste = await _helper.DurumaGoreGetir(durumKodu);
        return Ok(liste);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var bakim = await _helper.BakimGetir(id);

        if (bakim == null)
            return NotFound("Bakım planı bulunamadı.");

        return Ok(bakim);
    }
   
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] BakimPlani bakim)
    {
        var sonuc = await _helper.BakimEkle(bakim);
        return Ok(sonuc);
    }
  
    [HttpPut("{id}/durum")]
    public async Task<IActionResult> DurumGuncelle(string id, [FromBody] BakimDurumDto dto)
    {
        var sonuc = await _helper.DurumGuncelle(id, dto.DurumKodu, dto.IslemNotu);

        if (sonuc == null)
            return NotFound("Bakım planı bulunamadı.");

        return Ok(sonuc);
    }

    [HttpPut("{id}/ertele")]
    public async Task<IActionResult> Ertele(string id, [FromBody] BakimErteleDto dto)
    {
        var sonuc = await _helper.Ertele(id, dto.YeniTarih, dto.IslemNotu);

        if (sonuc == null)
            return NotFound("Bakım planı bulunamadı.");

        return Ok(sonuc);
    }
   
    [HttpPut("{id}/tamamla")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Tamamla(string id, [FromForm] BakimTamamlaDto dto)
    {
        List<int> personelIdleri = (dto.PersonelIdleri ?? "")
            .Split(",", StringSplitOptions.RemoveEmptyEntries)
            .Select(x => int.Parse(x.Trim()))
            .ToList();

        var sonuc = await _helper.BakimTamamla(
            id,
            personelIdleri,
            dto.IslemNotu ?? "",
            dto.OncesiResim,
            dto.SonrasiResim
        );

        if (sonuc == null)
            return NotFound("Bakım planı bulunamadı.");

        return Ok(sonuc);
    }
 
    [HttpGet("{id}/detaylar")]
    public async Task<IActionResult> Detaylar(string id)
    {
        var detaylar = await _helper.BakimDetaylariGetir(id);
        var personeller = await _personelHelper.TumPersonelleriGetir();

        foreach (var d in detaylar)
        {
            var personel = personeller
                .FirstOrDefault(x => x.Id == d.PersonelNo);

            if (personel != null)
            {
                d.AdSoyad = $"{personel.Ad} {personel.Soyad}";
            }
        }

        return Ok(detaylar);
    }

}

public class BakimDurumDto
{
    public string DurumKodu { get; set; } = "B";
    public string IslemNotu { get; set; } = string.Empty;
}

public class BakimErteleDto
{
    public DateTime YeniTarih { get; set; }
    public string IslemNotu { get; set; } = string.Empty;
}
public class BakimTamamlaDto
{
    public string? PersonelIdleri { get; set; }
    public string? IslemNotu { get; set; }
    public IFormFile? OncesiResim { get; set; }
    public IFormFile? SonrasiResim { get; set; }
}