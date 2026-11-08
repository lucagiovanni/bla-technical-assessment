using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using TaskManager.Api.Controllers;
using TaskManager.Application.Users;
using TaskManager.Infrastructure.Persistence;

namespace TaskManager.Api.Tests;

public class UsersApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public UsersApiTests(ApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_and_login_are_public_and_login_returns_a_token()
    {
        var email = $"ada-{Guid.NewGuid():N}@example.com";

        var register = await _client.PostAsJsonAsync("/api/users/register", new { email, password = "Secret123" });
        Assert.Equal(HttpStatusCode.Created, register.StatusCode);
        var created = await register.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(created);
        Assert.Equal(email, created.Email);
        Assert.NotEqual(Guid.Empty, created.Id);

        var login = await _client.PostAsJsonAsync("/api/users/login", new { email = email.ToUpperInvariant(), password = "Secret123" });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var signedIn = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(signedIn);
        Assert.Equal(created.Id, signedIn.Id);
        Assert.Equal(email, signedIn.Email);

        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(signedIn.Token);
        Assert.Equal("TaskManager", jwt.Issuer);
        Assert.Equal(created.Id.ToString(), jwt.Subject);
        Assert.Equal(email, jwt.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Email).Value);
    }

    [Fact]
    public async Task Seeded_demo_user_can_log_in()
    {
        var login = await _client.PostAsJsonAsync(
            "/api/users/login",
            new { email = DemoSeed.Email, password = DemoSeed.Password });

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var signedIn = await login.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(signedIn);
        Assert.Equal(DemoSeed.UserId, signedIn.Id);
        Assert.Equal(DemoSeed.Email, signedIn.Email);
        Assert.False(string.IsNullOrWhiteSpace(signedIn.Token));
    }

    [Fact]
    public async Task Duplicate_registration_conflicts()
    {
        var email = $"dup-{Guid.NewGuid():N}@example.com";
        var body = new { email, password = "Secret123" };

        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/users/register", body)).StatusCode);

        var duplicate = await _client.PostAsJsonAsync("/api/users/register", body);
        Assert.Equal(HttpStatusCode.Conflict, duplicate.StatusCode);
        var message = await duplicate.Content.ReadFromJsonAsync<ApiMessage>();
        Assert.Equal(RegisterUser.DuplicateMessage, message?.Message);
    }

    [Fact]
    public async Task Invalid_registration_and_bad_credentials_are_rejected()
    {
        var invalid = await _client.PostAsJsonAsync("/api/users/register", new { email = "not-an-email", password = "Secret123" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var invalidMessage = await invalid.Content.ReadFromJsonAsync<ApiMessage>();
        Assert.Equal("Email is not valid.", invalidMessage?.Message);

        var weak = await _client.PostAsJsonAsync("/api/users/register", new { email = "ada@example.com", password = "short" });
        Assert.Equal(HttpStatusCode.BadRequest, weak.StatusCode);

        var unknown = await _client.PostAsJsonAsync("/api/users/login", new { email = "missing@example.com", password = "Secret123" });
        var wrong = await _client.PostAsJsonAsync("/api/users/login", new { email = DemoSeed.Email, password = "Wrong123!" });
        Assert.Equal(HttpStatusCode.Unauthorized, unknown.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        var unknownMessage = await unknown.Content.ReadFromJsonAsync<ApiMessage>();
        var wrongMessage = await wrong.Content.ReadFromJsonAsync<ApiMessage>();
        Assert.Equal(LoginUser.InvalidCredentialsMessage, unknownMessage?.Message);
        Assert.Equal(unknownMessage?.Message, wrongMessage?.Message);
    }

    [Fact]
    public async Task Development_cors_allows_the_angular_dev_server_only()
    {
        using var allowed = new HttpRequestMessage(HttpMethod.Options, "/api/users/login");
        allowed.Headers.Add("Origin", "http://localhost:4200");
        allowed.Headers.Add("Access-Control-Request-Method", "POST");
        var allowedResponse = await _client.SendAsync(allowed);
        Assert.Equal(HttpStatusCode.NoContent, allowedResponse.StatusCode);
        Assert.Equal("http://localhost:4200", allowedResponse.Headers.GetValues("Access-Control-Allow-Origin").Single());

        using var blocked = new HttpRequestMessage(HttpMethod.Post, "/api/users/login");
        blocked.Headers.Add("Origin", "https://evil.example");
        blocked.Content = JsonContent.Create(new { email = DemoSeed.Email, password = "Wrong123!" });
        var blockedResponse = await _client.SendAsync(blocked);
        Assert.Equal(HttpStatusCode.Unauthorized, blockedResponse.StatusCode);
        Assert.False(blockedResponse.Headers.Contains("Access-Control-Allow-Origin"));
    }
}
