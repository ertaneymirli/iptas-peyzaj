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

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var personel = await _personelHelper.PersonelGetir(id);

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

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] Personel personel)
    {
        var sonuc = await _personelHelper.PersonelGuncelle(id, personel);

        if (sonuc == null)
            return NotFound("Personel bulunamadı.");

        return Ok(sonuc);
    }

    [HttpPut("{id:int}/durum")]
    public async Task<IActionResult> DurumGuncelle(int id, [FromBody] PersonelDurumDto dto)
    {
        var sonuc = await _personelHelper.DurumGuncelle(id, dto.DurumKodu);

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
