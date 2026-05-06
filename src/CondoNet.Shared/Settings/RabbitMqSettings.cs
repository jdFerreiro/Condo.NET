namespace CondoNet.Shared.Settings
{
    public class RabbitMqSettings
    {
        public string Host { get; set; } = string.Empty;
        public string VirtualHost { get; set; } = "/";
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public int Port { get; set; } = 5673;

        // Propiedad calculada para facilitar la conexión con MassTransit o el cliente oficial
        public string ConnectionString
        {
            get
            {
                var user = Uri.EscapeDataString(Username);
                var pass = Uri.EscapeDataString(Password);
                return $"amqp://{user}:{pass}@{Host}:{Port}{VirtualHost}";
            }
        }
    }
}
