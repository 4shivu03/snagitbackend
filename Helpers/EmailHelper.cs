using System.Net;
using System.Net.Mail;

public class EmailHelper
{
    private readonly IConfiguration _config;
    public EmailHelper(IConfiguration config)
    {
        _config = config;
    }

    public async Task SendOtpEmail(string toEmail, string otp)
    {
        var fromEmail = _config["EmailSettings:Email"];
        var password = _config["EmailSettings:Password"];
        var host = _config["EmailSettings:Host"];
        var port = int.Parse(_config["EmailSettings:Port"]);
        var smtp = new SmtpClient(host, port)
        {
            EnableSsl = true,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(fromEmail, password)
        };

        var mail = new MailMessage
        {
            From = new MailAddress(fromEmail), 
            Subject = "Password Reset OTP",
            Body = $"Your OTP is: {otp}\n\nIt will expire in 10 minutes.",
            IsBodyHtml = false
        };

        mail.To.Add(toEmail);
        await smtp.SendMailAsync(mail);
    }
}