using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Kinovea.ApiGetway.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly Kinovea.ApiGetway.ServiceRegistry.ServiceRegistry  _registry;
        private readonly HealthCheckService _healthCheck;

        [HttpGet]
        public async Task<IActionResult> GetServiceStatus()
        {
            var services = _registry.GetServices();
            var health = await _healthCheck.CheckHealthAsync();

            return Ok(new
            {
                Services = services,
                Health = health
            });
        }
    }
}
