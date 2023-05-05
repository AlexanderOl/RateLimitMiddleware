namespace RateLimitApp.Models
{
    public class IsPrimeNumberResponse : BaseResponse
    {
        public bool IsPrime { get; set; }
        public int Number { get; set; }
    }
}
