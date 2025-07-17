namespace Sberkorus.Cbr.Infrastructure.Configuration
{
    public class RedisOptions
    {
        public string ConnectionString { get; set; }
        
        public string Password { get; set; }
        
        public int UserSessionExpirationHours { get; set; }
    }
}