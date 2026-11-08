using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TaskManager.Api.Controllers;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Api.Tests;

public class CurrentUserApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CurrentUserApiTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Returns_the_signed_in_user()
    {
        var token = await LoginAsync(DemoSeed.Email, DemoSeed.Password);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(user);
        Assert.Equal(DemoSeed.UserId, user.Id);
        Assert.Equal(DemoSeed.Email, user.Email);
    }

    [Fact]
    public async Task Rejects_a_missing_token()
    {
        var response = await _client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Theory]
    [InlineData("not-a-token")]
    [InlineData("")]
    public async Task Rejects_an_invalid_token(string token)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Rejects_a_token_signed_with_a_different_key()
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("different-demo-signing-key-32-bytes!"));
        var jwt = new JwtSecurityToken(
            issuer: "TaskManager",
            audience: "TaskManager",
            claims: [new Claim(JwtRegisteredClaimNames.Sub, DemoSeed.UserId.ToString())],
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256));
        var token = new JwtSecurityTokenHandler().WriteToken(jwt);

        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var response = await _client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private async Task<string> LoginAsync(string email, string password)
    {
        var response = await _client.PostAsJsonAsync("/api/users/login", new { email, password });
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(body);
        return body.Token;
    }
}
