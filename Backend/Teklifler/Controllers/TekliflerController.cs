using IptasPeyzajApi.Backend.Teklifler.Helpers;
using IptasPeyzajApi.Backend.Teklifler.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IptasPeyzajApi.Backend.Teklifler.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TekliflerController : ControllerBase
{
    private readonly TeklifHelper _helper;

    public TekliflerController(TeklifHelper helper)
    {
        _helper = helper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var liste = await _helper.TumTeklifleriGetir();
        return Ok(liste);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var teklif = await _helper.TeklifGetir(id);

        if (teklif == null)
            return NotFound("Teklif bulunamadı.");

        return Ok(teklif);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Teklif teklif)
    {
        var sonuc = await _helper.TeklifEkle(teklif);
        return Ok(sonuc);
    }

    [HttpPut("{id}/durum")]
    public async Task<IActionResult> DurumGuncelle(string id, [FromBody] TeklifDurumDto dto)
    {
        var sonuc = await _helper.DurumGuncelle(id, dto.DurumKodu, dto.IslemNotu);

        if (sonuc == null)
            return NotFound("Teklif bulunamadı.");

        return Ok(sonuc);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var sonuc = await _helper.TeklifSil(id);

        if (sonuc == null)
            return NotFound("Teklif bulunamadı.");

        return Ok(sonuc);
    }
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Teklif teklif)
    {
        var sonuc = await _helper.TeklifGuncelle(id, teklif);

        if (sonuc == null)
            return NotFound("Teklif bulunamadı.");

        return Ok(sonuc);
    }
    [HttpGet("silinenler")]
    public async Task<IActionResult> Silinenler()
    {
        var liste = await _helper.PasifTeklifleriGetir();

        return Ok(liste);
    }
}

public class TeklifDurumDto
{
    public string DurumKodu { get; set; } = "B";
    public string IslemNotu { get; set; } = string.Empty;
}
