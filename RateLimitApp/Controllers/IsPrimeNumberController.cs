using Microsoft.AspNetCore.Mvc;
using RateLimitApp.Interfaces;
using RateLimitApp.Models;


namespace RateLimitApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class IsPrimeNumberController : ControllerBase
    {
        
        private readonly IPrimeNumberService _rateLimitService;

        public IsPrimeNumberController(IPrimeNumberService rateLimitService)
        {
            _rateLimitService = rateLimitService;
        }

        [HttpPost(Name = "CheckPrimeNumber")]
        [LimitRequest]
        public IActionResult Post([FromBody] IsPrimeNumberRequest req)
        {
            if (!ModelState.IsValid)
            {
                return UnprocessableEntity(ModelState);
            }

            var isPrime = _rateLimitService.IsPrime(req.Number);

            return Ok(new IsPrimeNumberResponse
            {
                Number = req.Number,
                IsPrime = isPrime
            });
        }

    }
}