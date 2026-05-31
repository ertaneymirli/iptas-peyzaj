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
}