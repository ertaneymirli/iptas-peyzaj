using IptasPeyzajApi.Backend.Kullanicilar.Helpers;
using IptasPeyzajApi.Backend.kullanici.Model;
using IptasPeyzajApi.Backend.kullanici.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IptasPeyzajApi.Backend.kullanici.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KullanicilarController : ControllerBase
{
    private readonly KullaniciHelper _helper;
    private readonly IConfiguration _configuration;

    public KullanicilarController(KullaniciHelper helper, IConfiguration configuration)
    {
        _helper = helper;
        _configuration = configuration;
    }

    [Authorize(Roles = "1")]
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _helper.TumKullanicilariGetir());

    [Authorize(Roles = "1")]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        Kullanici? kullanici = await _helper.KullaniciGetir(id);
        return kullanici == null ? NotFound("Kullanıcı bulunamadı.") : Ok(kullanici);
    }

    [Authorize(Roles = "1")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] KullaniciCreateDto dto)
    {
        try { return Ok(await _helper.KullaniciEkle(dto)); }
        catch (ArgumentException ex) { return BadRequest(ex.Message); }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [Authorize(Roles = "1")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] KullaniciUpdateDto dto)
    {
        try
        {
            Kullanici? kullanici = await _helper.KullaniciGuncelle(id, dto);
            return kullanici == null ? NotFound("Kullanıcı bulunamadı.") : Ok(kullanici);
        }
        catch (InvalidOperationException ex) { return Conflict(ex.Message); }
    }

    [Authorize(Roles = "1")]
    [HttpPut("{id:int}/durum")]
    public async Task<IActionResult> DurumGuncelle(int id, [FromBody] KullaniciDurumDto dto)
    {
        Kullanici? kullanici = await _helper.DurumGuncelle(id, dto.AktifMi);
        return kullanici == null ? NotFound("Kullanıcı bulunamadı.") : Ok("Kullanıcı durumu güncellendi.");
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        Kullanici? kullanici = await _helper.GirisYap(dto);
        if (kullanici == null)
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        string jwtKey = _configuration["JWT_KEY"]
            ?? Environment.GetEnvironmentVariable("JWT_KEY")
            ?? throw new InvalidOperationException("JWT_KEY ayarı bulunamadı.");

        JwtSecurityTokenHandler tokenHandler = new();
        SecurityTokenDescriptor descriptor = new()
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("id", kullanici.Id.ToString()),
                new Claim("kullaniciAdi", kullanici.KullaniciAdi),
                new Claim(ClaimTypes.Role, kullanici.Rol)
            }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        SecurityToken token = tokenHandler.CreateToken(descriptor);
        return Ok(new
        {
            Mesaj = "Giriş başarılı",
            Token = tokenHandler.WriteToken(token),
            Kullanici = kullanici
        });
    }
}

public class KullaniciDurumDto
{
    public bool AktifMi { get; set; }
}
