using IptasPeyzajApi.Backend.MusteriKullanicilari.Helpers;
using IptasPeyzajApi.Backend.MusteriKullanicilari.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IptasPeyzajApi.Backend.MusteriKullanicilari.Controllers;

[Authorize(Roles = "1")]
[ApiController]
[Route("api/[controller]")]
public class MusteriKullanicilariController : ControllerBase
{
    private readonly MusteriKullanicisiHelper
        _musteriKullanicisiHelper;

    public MusteriKullanicilariController(
        MusteriKullanicisiHelper musteriKullanicisiHelper)
    {
        _musteriKullanicisiHelper =
            musteriKullanicisiHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var sonuc = await _musteriKullanicisiHelper
            .TumBaglantilariGetir();

        return Ok(sonuc);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var sonuc = await _musteriKullanicisiHelper
            .BaglantiGetir(id);

        if (sonuc == null)
            return NotFound(
                "Müşteri-kullanıcı bağlantısı bulunamadı.");

        return Ok(sonuc);
    }

    [HttpGet("kullanici/{kullaniciId}")]
    public async Task<IActionResult> GetByKullanici(
        int kullaniciId)
    {
        var sonuc = await _musteriKullanicisiHelper
            .KullaniciyaGoreGetir(kullaniciId);

        return Ok(sonuc);
    }

    [HttpGet("musteri/{musteriId}")]
    public async Task<IActionResult> GetByMusteri(
        int musteriId)
    {
        var sonuc = await _musteriKullanicisiHelper
            .MusteriyeGoreGetir(musteriId);

        return Ok(sonuc);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] MusteriKullanicisi model)
    {
        try
        {
            var sonuc = await _musteriKullanicisiHelper
                .BaglantiGuncelle(id, model);
            return sonuc == null
                ? NotFound("Müşteri-kullanıcı bağlantısı bulunamadı.")
                : Ok(sonuc);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] MusteriKullanicisi model)
    {
        try
        {
            var sonuc = await _musteriKullanicisiHelper
                .BaglantiEkle(model);

            return Ok(sonuc);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        bool silindiMi = await _musteriKullanicisiHelper
            .BaglantiSil(id);

        if (!silindiMi)
            return NotFound(
                "Müşteri-kullanıcı bağlantısı bulunamadı.");

        return Ok(
            "Müşteri-kullanıcı bağlantısı silindi.");
    }

    [HttpDelete(
        "kullanici/{kullaniciId}/musteri/{musteriId}")]
    public async Task<IActionResult> DeleteByKullaniciMusteri(
        int kullaniciId,
        int musteriId)
    {
        bool silindiMi = await _musteriKullanicisiHelper
            .KullaniciMusteriBaglantisiSil(
                kullaniciId,
                musteriId);

        if (!silindiMi)
            return NotFound(
                "Müşteri-kullanıcı bağlantısı bulunamadı.");

        return Ok(
            "Müşteri-kullanıcı bağlantısı silindi.");
    }
}
