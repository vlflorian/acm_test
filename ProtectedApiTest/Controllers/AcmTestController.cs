using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ProtectedApiTest.Controllers;

[Route("[controller]")]
public class AcmTestController : Controller
{
    [HttpGet("protected")]
    [Authorize]
    [ProducesResponseType(typeof(ClaimsPrincipal), (int)HttpStatusCode.OK)]
    public IActionResult ProtectedEndpoint()
    {
        var claims = HttpContext.User.Claims.Select(c => new { c.Type, c.Value }).ToDictionary(x => x.Type, y => y.Value);
        return Ok(claims);
    }

    [HttpGet("unprotected")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ClaimsPrincipal), (int)HttpStatusCode.OK)]
    public IActionResult UnProtectedEndpoint()
    {
        return Ok("Unprotected endpoint OK");
    }
}