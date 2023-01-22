namespace App.Login.Features.Email;

public class EmailSender : IEmailSender
{
  public Task Send(string email, string subject, string message, CancellationToken cancellationToken = default)
  {
    throw new NotImplementedException();
  }
}
