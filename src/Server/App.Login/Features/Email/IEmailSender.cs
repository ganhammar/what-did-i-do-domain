namespace App.Login.Features.Email;

public interface IEmailSender
{
  Task Send(string email, string subject, string message, CancellationToken cancellationToken = default);
}
