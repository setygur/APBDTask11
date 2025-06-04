namespace APBDTask11.WebApp;

public interface ITokenService
{
    string GenerateToken(string username, string role);
}