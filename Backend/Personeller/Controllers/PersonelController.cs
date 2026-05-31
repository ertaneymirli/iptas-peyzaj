using IptasPeyzajApi.Backend.Personeller.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IptasPeyzajApi.Backend.Personeller.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class PersonellerController : ControllerBase
{
    private readonly PersonelHelper _personelHelper;

    public PersonellerController(PersonelHelper personelHelper)
    {
        _personelHelper = personelHelper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var liste = await _personelHelper.TumPersonelleriGetir();
        return Ok(liste);
    }

    [HttpGet("{docId}")]
    public async Task<IActionResult> GetById(string docId)
    {
        var personel = await _personelHelper.PersonelGetir(docId);

        if (personel == null)
            return NotFound("Personel bulunamadı.");

        return Ok(personel);
    }
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Personel personel)
    {
        var sonuc = await _personelHelper.PersonelEkle(personel);
        return Ok(sonuc);
    }

    [HttpPut("{docId}")]
    public async Task<IActionResult> Update(string docId, [FromBody] Personel personel)
    {
        var sonuc = await _personelHelper.PersonelGuncelle(docId, personel);

        if (sonuc == null)
            return NotFound("Personel bulunamadı.");

        return Ok(sonuc);
    }

    [HttpPut("{docId}/durum")]
    public async Task<IActionResult> DurumGuncelle(string docId, [FromBody] PersonelDurumDto dto)
    {
        var sonuc = await _personelHelper.DurumGuncelle(docId, dto.DurumKodu);

        if (sonuc == null)
            return NotFound("Personel bulunamadı.");

        return Ok(sonuc);
    }

    [HttpGet("durum/{durumKodu}")]
    public async Task<IActionResult> GetByDurum(string durumKodu)
    {
        var liste = await _personelHelper.DurumaGoreGetir(durumKodu);
        return Ok(liste);
    }
}
public class PersonelDurumDto
{
    public string DurumKodu { get; set; } = "A";
}