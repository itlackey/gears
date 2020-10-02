namespace Gears
{
    internal class SmtpConfiguration
    {
        public string Host { get; set; }
        public string FromAddress { get; set; }
        public int Port { get; set; }
        public bool EnableSSL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public bool ThrottleEmail { get; set; } = true;
    }
}