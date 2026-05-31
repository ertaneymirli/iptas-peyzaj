using IptasPeyzajApi.Backend.Musteriler.Helpers;
using IptasPeyzajApi.Backend.Musteriler.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IptasPeyzajApi.Backend.Musteriler.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class MusterilerController : ControllerBase
{
    private readonly MusteriHelper _musteriHelper;

    public MusterilerController(MusteriHelper musteriHelper)
    {
        _musteriHelper = musteriHelper;
    }
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sonuc = await _musteriHelper.TumMusterileriGetir();
        return Ok(sonuc);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var musteri = await _musteriHelper.MusteriGetir(id);

        if (musteri == null)
            return NotFound("Müşteri bulunamadı.");

        return Ok(musteri);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Musteri musteri)
    {
        var sonuc = await _musteriHelper.MusteriEkle(musteri);
        return Ok(sonuc);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] Musteri musteri)
    {
        var sonuc = await _musteriHelper.MusteriGuncelle(id, musteri);

        if (sonuc == null)
            return NotFound("Müşteri bulunamadı.");

        return Ok(sonuc);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var silindiMi = await _musteriHelper.MusteriSil(id);

        if (!silindiMi)
            return NotFound("Müşteri bulunamadı.");

        return Ok("Müşteri silindi.");
    }
    [HttpPut("{id}/durum")]
    public async Task<IActionResult> DurumDegistir(string id, [FromBody] MusteriDurumDto dto)
    {
        var sonuc = await _musteriHelper.MusteriDurumDegistir(id, dto.DurumKodu);

        if (!sonuc)
            return NotFound("Müşteri bulunamadı.");

        return Ok("Durum güncellendi.");
    }

    public class MusteriDurumDto
    {
        public string DurumKodu { get; set; } = "P";
    }
    [HttpGet("durum/{durumKodu}")]
    public async Task<IActionResult> GetByDurum(string durumKodu)
    {
        var liste = await _musteriHelper.MusterileriDurumaGoreGetir(durumKodu);
        return Ok(liste);
    }
}