using System.ComponentModel.DataAnnotations;

namespace RateLimitApp.Models
{
    public class IsPrimeNumberRequest
    {
        [Required]
        public int Number { get; set; }
        [Required]
        [MaxLength(100)]
        public string Token { get; set; }
    }
}
