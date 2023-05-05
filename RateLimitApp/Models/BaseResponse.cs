namespace RateLimitApp.Models
{
    public class BaseResponse
    {
        public bool HasErrors { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
