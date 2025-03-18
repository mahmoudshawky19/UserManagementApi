using Microsoft.AspNetCore.Mvc.Testing;
using UserManagementApi;
using Xunit;

public class ErrorHandlingTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ErrorHandlingTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task TestErrorHandling_Returns500()
    {
        // Act
        var response = await _client.GetAsync("/api/account/test-error");

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.InternalServerError, response.StatusCode);
        var content = await response.Content.ReadAsStringAsync();
        Assert.Contains("This is a test error.", content);
    }
}
