using System.Net;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using Xunit;

namespace App.Authorizer.Test;

public class TokenClientTest
{
  [Fact]
  public async Task Should_BeSuccessfulResponse_When_RequestIsValid()
  {
    var tokenClient = GetTokenClient();
    var result = await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientId = "test",
      ClientSecret = "test",
      Issuer = "https://test.com",
    }, "123");

    Assert.True(result.Active);
  }

  [Fact]
  public async Task Should_BeSuccessfulResponse_When_TokenIsCached()
  {
    var tokenClient = GetTokenClient(true);
    var result = await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientId = "test",
      ClientSecret = "test",
      Issuer = "https://test.com",
    }, "123");

    Assert.True(result.Active);
  }

  [Fact]
  public async Task Should_Throw_When_IssuerIsNotSet()
  {
    var tokenClient = GetTokenClient();
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientId = "test",
      ClientSecret = "test",
    }, "123"));

    Assert.Equal(nameof(AuthorizationOptions.Issuer), exception.ParamName);
  }

  [Fact]
  public async Task Should_Throw_When_ClientIdIsNotSet()
  {
    var tokenClient = GetTokenClient();
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientSecret = "test",
      Issuer = "https://test.com",
    }, "123"));

    Assert.Equal(nameof(AuthorizationOptions.ClientId), exception.ParamName);
  }

  [Fact]
  public async Task Should_Throw_When_ClientSecretIsNotSet()
  {
    var tokenClient = GetTokenClient();
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientId = "test",
      Issuer = "https://test.com",
    }, "123"));

    Assert.Equal(nameof(AuthorizationOptions.ClientSecret), exception.ParamName);
  }

  [Fact]
  public async Task Should_Throw_When_AudiencesIsNotSet()
  {
    var tokenClient = GetTokenClient();
    var exception = await Assert.ThrowsAsync<ArgumentNullException>(async () => await tokenClient.Validate(new()
    {
      ClientId = "test",
      ClientSecret = "test",
      Issuer = "https://test.com",
    }, "123"));

    Assert.Equal(nameof(AuthorizationOptions.Audiences), exception.ParamName);
  }

  [Fact]
  public async Task Should_Throw_When_TokenRequestIsNotValid()
  {
    var tokenClient = GetTokenClient(tokenRequestIsSuccessful: false);
    var exception = await Assert.ThrowsAsync<Exception>(async () => await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientId = "test",
      ClientSecret = "test",
      Issuer = "https://test.com",
    }, "123"));

    Assert.Equal("Could not get token for introspection", exception.Message);
  }

  [Fact]
  public async Task Should_Throw_When_IntrospectionRequestIsNotValid()
  {
    var tokenClient = GetTokenClient(tokenIntrospectionIsSuccessful: false);
    await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientId = "test",
      ClientSecret = "test",
      Issuer = "https://test.com",
    }, "123"));
  }

  [Fact]
  public async Task Should_Throw_When_TokenIsNotActive()
  {
    var tokenClient = GetTokenClient(isActive: false);
    await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientId = "test",
      ClientSecret = "test",
      Issuer = "https://test.com",
    }, "123"));
  }

  [Fact]
  public async Task Should_Throw_When_TokenUsageIsNotAccess()
  {
    var tokenClient = GetTokenClient(tokenUsage: "id_token");
    await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientId = "test",
      ClientSecret = "test",
      Issuer = "https://test.com",
    }, "123"));
  }

  [Fact]
  public async Task Should_Throw_When_AudienceIsNotCorrect()
  {
    var tokenClient = GetTokenClient(audience: "tset");
    await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await tokenClient.Validate(new()
    {
      Audiences = new() { "test" },
      ClientId = "test",
      ClientSecret = "test",
      Issuer = "https://test.com",
    }, "123"));
  }

  private TokenClient GetTokenClient(
    bool tokenIsCached = false,
    bool tokenRequestIsSuccessful = true,
    bool tokenIntrospectionIsSuccessful = true,
    bool isActive = true,
    string tokenUsage = "access_token",
    string audience = "test")
  {
    var expiresIn = 60 * 30;
    var httpMessageHandlerMock = new Mock<HttpMessageHandler>(MockBehavior.Strict);
    httpMessageHandlerMock
      .Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(x => x.Method == HttpMethod.Post),
        ItExpr.IsAny<CancellationToken>()
      )
      .ReturnsAsync(new HttpResponseMessage()
      {
        StatusCode = tokenRequestIsSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
        Content = tokenRequestIsSuccessful
          ? new StringContent($"{{\"access_token\":\"123\",\"expires_in\":{expiresIn}}}")
          : new StringContent("{\"error\":\"Invalid request\"}"),
      });

    var tokenEndpoint = "https://test.com/connect/token";
    var introspectEndpoint = "https://test.com/connect/introspect";

    httpMessageHandlerMock
      .Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(x =>
          x.Method == HttpMethod.Get &&
          x.RequestUri != default &&
          x.RequestUri.AbsolutePath.Contains("connect/introspect") == false),
        ItExpr.IsAny<CancellationToken>()
      )
      .ReturnsAsync(new HttpResponseMessage()
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent(
          $"{{\"token_endpoint\":\"{tokenEndpoint}\",\"introspection_endpoint\":\"{introspectEndpoint}\"}}"),
      });

    var isActiveString = isActive ? "true" : "false";
    httpMessageHandlerMock
      .Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(x =>
          x.Method == HttpMethod.Get &&
          x.RequestUri != default &&
          x.RequestUri.AbsolutePath.Contains("connect/introspect") == true),
        ItExpr.IsAny<CancellationToken>()
      )
      .ReturnsAsync(new HttpResponseMessage()
      {
        StatusCode = tokenIntrospectionIsSuccessful ? HttpStatusCode.OK : HttpStatusCode.BadRequest,
        Content = tokenIntrospectionIsSuccessful
          ? new StringContent(
            $"{{\"active\":{isActiveString},\"aud\":\"{audience}\",\"token_usage\":\"{tokenUsage}\",\"sub\":\"123\",\"scope\":\"test\"}}")
          : new StringContent("{\"error\":\"Invalid request\"}"),
      });

    var memoryCacheMock = new Mock<IMemoryCache>();
    var mockCacheEntry = new Mock<ICacheEntry>();
    object? value = tokenIsCached ? "456" : null;
    string? keyPayload = null;
    memoryCacheMock.Setup(x => x.TryGetValue(It.IsAny<string>(), out value)).Returns(tokenIsCached);
    memoryCacheMock
      .Setup(x => x.CreateEntry(It.IsAny<object>()))
      .Callback((object k) => keyPayload = (string)k)
      .Returns(mockCacheEntry.Object);

    return new TokenClient(new HttpClient(httpMessageHandlerMock.Object), memoryCacheMock.Object);
  }
}
