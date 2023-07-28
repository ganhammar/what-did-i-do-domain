using System.Globalization;

namespace App.Api.Shared.Extensions;

public static class DateTimeExtensions
{
  public static string? ToUniversalString(this DateTime? date) => date?
    .ToUniversalString();

  public static string ToUniversalString(this DateTime date) => date
    .ToUniversalTime()
    .ToString("yyyy-MM-dd'T'HH:mm:ss.fffK", CultureInfo.InvariantCulture);
}
