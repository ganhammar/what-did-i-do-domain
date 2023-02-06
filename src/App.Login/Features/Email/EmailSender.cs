using System.Net;
using Amazon.SimpleEmail;
using Amazon.SimpleEmail.Model;

namespace App.Login.Features.Email;

public class EmailSender : IEmailSender
{
  public async Task Send(string email, string subject, string message, CancellationToken cancellationToken = default)
  {
    var client = new AmazonSimpleEmailServiceClient();
    var response = await client.SendEmailAsync(new SendEmailRequest(
      Constants.FromEmail,
      new Destination(new[] { email }.ToList()),
      new Message(new Content(subject), new Body(new Content(message)))));

    if (response.HttpStatusCode != HttpStatusCode.OK)
    {
      throw new Exception($"Could not send email, status code {response.HttpStatusCode}");
    }
  }
}
