using System.Text.Json;
using System.Text.Json.Serialization;
using Amazon.DynamoDBv2.Model;

namespace App.Api.ListEvents;

public class AttributeValueJsonConverter : JsonConverter<AttributeValue>
{
  public override AttributeValue Read(
    ref Utf8JsonReader reader,
    Type typeToConvert,
    JsonSerializerOptions options) => new AttributeValue(reader.GetString());

  public override void Write(
    Utf8JsonWriter writer,
    AttributeValue attributeValue,
    JsonSerializerOptions options) => writer.WriteStringValue(attributeValue.S);
}
