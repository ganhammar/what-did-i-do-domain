using Amazon.Lambda.Annotations;
using Amazon.Lambda.Annotations.APIGateway;

namespace App.Api.CreateEvent;

public class Function
{
    [LambdaFunction]
    public string Process([FromBody] string name)
    {
        return name;
    }
}