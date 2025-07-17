namespace Sberkorus.Cbr.Infrastructure.Configuration
{
    /// <summary>
    /// Настройки подключения к Redis
    /// </summary>
    public class RedisOptions
    {
        /// <summary>
        /// Строка подключения к серверу Redis
        /// </summary>
        public string ConnectionString { get; set; }
        
        /// <summary>
        /// Пароль для подключения к Redis
        /// </summary>
        public string Password { get; set; }
        
        /// <summary>
        /// Время жизни пользовательской сессии в часах
        /// </summary>
        public int UserSessionExpirationHours { get; set; }
    }
}