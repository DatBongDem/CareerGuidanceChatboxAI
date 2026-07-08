namespace BusinessLogic.DTOs.Email
{
    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string SenderEmail { get; set; }
        public string Username { get; set; } = string.Empty;
        public string AppPassword { get; set; }
    }
}
