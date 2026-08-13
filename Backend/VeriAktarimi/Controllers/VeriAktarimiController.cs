using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IptasPeyzajApi.Backend.VeriAktarimi.Controllers;

[Authorize(Roles = "1")]
[ApiController]
[Route("api/[controller]")]
public sealed class VeriAktarimiController : ControllerBase
{
    private readonly FirestoreToSqlMigrationService _service;
    private readonly IConfiguration _configuration;

    public VeriAktarimiController(
        FirestoreToSqlMigrationService service,
        IConfiguration configuration)
    {
        _service = service;
        _configuration = configuration;
    }

    [HttpPost("firestore-to-sql")]
    public async Task<IActionResult> FirestoreToSql(CancellationToken ct)
    {
        if (!_configuration.GetValue<bool>("DataMigration:Enabled"))
            return NotFound();

        var sonuc = await _service.AktarAsync(ct);
        return Ok(sonuc);
    }

    [HttpGet("sql-sayilari")]
    public async Task<IActionResult> SqlSayilari(CancellationToken ct)
    {
        if (!_configuration.GetValue<bool>("DataMigration:Enabled"))
            return NotFound();

        return Ok(await _service.SqlSayilariniGetir(ct));
    }
}
