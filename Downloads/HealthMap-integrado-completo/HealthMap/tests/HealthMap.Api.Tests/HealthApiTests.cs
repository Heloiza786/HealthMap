using System.Net;
using Xunit;

namespace HealthMap.Api.Tests;

public class HealthApiTests : ApiTestBase
{
    [Fact]
    public async Task Get_Health_RetornaOk()
    {
        var response = await Client.GetAsync("/api/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await ReadJsonAsync(response);
        Assert.Equal("ok", json.GetProperty("status").GetString());
    }
}
