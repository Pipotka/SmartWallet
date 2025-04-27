using Microsoft.AspNetCore.Mvc;

namespace Nasurino.SmartWallet.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class WeatherForecastController : ControllerBase
	{
		private readonly ILogger<WeatherForecastController> _logger;
		private readonly IConfiguration _config;

		public WeatherForecastController(ILogger<WeatherForecastController> logger, IConfiguration config)
		{
			_logger = logger;
			_config = config;
		}

		[HttpGet(Name = "GetConfig")]
		public IActionResult Get()
		{
			var conString = _config.GetSection("ApiSettings").GetValue<string>("ConnectionString");
			var jwtkey = _config.GetSection("ApiSettings").GetValue<string>("JWTKey");
			var result = $"ConnectionString: {conString}\n" +
				$"JWTKey: {jwtkey}\n";
			return Ok(result);
		}
	}
}
