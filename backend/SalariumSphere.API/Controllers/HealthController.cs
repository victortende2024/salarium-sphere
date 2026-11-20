using Microsoft.AspNetCore.Mvc;

namespace SalariumSphere.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Check()
        {
            return Ok(new { Status = "Salarium Sphere Phase 0 is running!", Timestamp = DateTime.UtcNow });
        }
    }
}
