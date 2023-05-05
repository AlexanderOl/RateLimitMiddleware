namespace RateLimitApp.Models
{
    [AttributeUsage(AttributeTargets.Method)]
    public class LimitRequest : Attribute { }
}
