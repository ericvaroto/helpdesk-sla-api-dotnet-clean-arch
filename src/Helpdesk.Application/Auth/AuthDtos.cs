namespace Helpdesk.Application.Auth
{
    public sealed record LoginRequest(string Email, string Password);

    public sealed record AuthResponse(string AccessToken, string RefreshToken);

    public sealed record RefreshRequest(string RefreshToken);

    public sealed record RevokeRequest(string RefreshToken);
}