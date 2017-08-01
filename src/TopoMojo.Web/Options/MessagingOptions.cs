using TopoMojo.Core;

namespace TopoMojo
{

    public class MessagingOptions
    {
        public EmailConfiguration Email { get; set; }
        public SMSConfiguration Text { get; set; }
    }

    public class EmailConfiguration
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string Sender { get; set; }
    }

    public class SMSConfiguration
    {

    }
}