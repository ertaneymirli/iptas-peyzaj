using Google.Cloud.Firestore;
using IptasPeyzajApi.Backend.kullanici.Model;
using IptasPeyzajApi.Backend.kullanici.Model.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace IptasPeyzajApi.Backend.kullanici.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class KullanicilarController : ControllerBase
{
    private readonly FirestoreDb _db;
    private const string CollectionName = "kullanicilar";

    public KullanicilarController(FirestoreDb db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .OrderByDescending("KayitTarihi")
            .GetSnapshotAsync();

        List<Kullanici> liste = new();

        foreach (DocumentSnapshot doc in snapshot.Documents)
        {
            Kullanici kullanici = doc.ConvertTo<Kullanici>();
            kullanici.Id = doc.Id;
            kullanici.SifreHash = "";
            liste.Add(kullanici);
        }

        return Ok(liste);
    }
  
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        DocumentSnapshot doc = await _db.Collection(CollectionName)
            .Document(id)
            .GetSnapshotAsync();

        if (!doc.Exists)
            return NotFound("Kullanıcı bulunamadı.");

        Kullanici kullanici = doc.ConvertTo<Kullanici>();
        kullanici.Id = doc.Id;
        kullanici.SifreHash = "";

        return Ok(kullanici);
    }
   
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] KullaniciCreateDto dto)
    {
        QuerySnapshot mevcut = await _db.Collection(CollectionName)
            .WhereEqualTo("KullaniciAdi", dto.KullaniciAdi)
            .Limit(1)
            .GetSnapshotAsync();

        if (mevcut.Count > 0)
            return BadRequest("Bu kullanıcı adı zaten kayıtlı.");

        Kullanici kullanici = new()
        {
            KullaniciAdi = dto.KullaniciAdi,
            Ad = dto.Ad,
            Soyad = dto.Soyad,
            DogumTarihi = UtcYap(dto.DogumTarihi),
            Tc = dto.Tc,
            TelefonNo = dto.TelefonNo,
            CepTelefonNo = dto.CepTelefonNo,
            Adres = dto.Adres,
            Mail = dto.Mail,
            SifreHash = Hashle(dto.Sifre),
            Rol = dto.Rol,
            AktifMi = true,
            KayitTarihi = DateTime.UtcNow
        };

        DocumentReference addedDoc = await _db.Collection(CollectionName).AddAsync(kullanici);
        kullanici.Id = addedDoc.Id;
        kullanici.SifreHash = "";

        return Ok(kullanici);
    }
   
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] KullaniciUpdateDto dto)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return NotFound("Kullanıcı bulunamadı.");

        Kullanici kullanici = doc.ConvertTo<Kullanici>();

        kullanici.KullaniciAdi = dto.KullaniciAdi;
        kullanici.Ad = dto.Ad;
        kullanici.Soyad = dto.Soyad;
        kullanici.DogumTarihi = UtcYap(dto.DogumTarihi);
        kullanici.Tc = dto.Tc;
        kullanici.TelefonNo = dto.TelefonNo;
        kullanici.CepTelefonNo = dto.CepTelefonNo;
        kullanici.Adres = dto.Adres;
        kullanici.Mail = dto.Mail;
        kullanici.Rol = dto.Rol;

        if (!string.IsNullOrWhiteSpace(dto.Sifre))
        {
            kullanici.SifreHash = Hashle(dto.Sifre);
        }

        await docRef.SetAsync(kullanici, SetOptions.Overwrite);

        kullanici.Id = id;
        kullanici.SifreHash = "";

        return Ok(kullanici);
    }
   
  
    [HttpPut("{id}/durum")]
    public async Task<IActionResult> DurumGuncelle(string id, [FromBody] KullaniciDurumDto dto)
    {
        DocumentReference docRef = _db.Collection(CollectionName).Document(id);
        DocumentSnapshot doc = await docRef.GetSnapshotAsync();

        if (!doc.Exists)
            return NotFound("Kullanıcı bulunamadı.");

        await docRef.UpdateAsync(new Dictionary<string, object>
    {
        { "AktifMi", dto.AktifMi }
    });

        return Ok("Kullanıcı durumu güncellendi.");
    }
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        string sifreHash = Hashle(dto.Sifre);

        QuerySnapshot snapshot = await _db.Collection(CollectionName)
            .WhereEqualTo("KullaniciAdi", dto.KullaniciAdi)
            .WhereEqualTo("SifreHash", sifreHash)
            .WhereEqualTo("AktifMi", true)
            .Limit(1)
            .GetSnapshotAsync();

        if (snapshot.Count == 0)
            return Unauthorized("Kullanıcı adı veya şifre hatalı.");

        DocumentSnapshot doc = snapshot.Documents[0];
        Kullanici kullanici = doc.ConvertTo<Kullanici>();
        kullanici.Id = doc.Id;
        kullanici.SifreHash = "";

        var tokenHandler = new JwtSecurityTokenHandler();

        var key = Encoding.UTF8.GetBytes("iptas-peyzaj-cok-gizli-anahtar-2026");

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
        new Claim("id", kullanici.Id ?? ""),
        new Claim("kullaniciAdi", kullanici.KullaniciAdi),
        new Claim(ClaimTypes.Role, kullanici.Rol)
    }),
            Expires = DateTime.UtcNow.AddHours(8),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature
            )
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return Ok(new
        {
            Mesaj = "Giriş başarılı",
            Token = tokenHandler.WriteToken(token),
            Kullanici = kullanici
        });
    }

    private static string Hashle(string sifre)
    {
        using SHA256 sha = SHA256.Create();
        byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(sifre));
        return Convert.ToHexString(bytes);
    }
    private static DateTime? UtcYap(DateTime? tarih)
    {
        if (tarih == null)
            return null;

        if (tarih.Value.Kind == DateTimeKind.Utc)
            return tarih;

        return DateTime.SpecifyKind(tarih.Value, DateTimeKind.Utc);
    }

}

public class KullaniciDurumDto
{
    public bool AktifMi { get; set; }
}