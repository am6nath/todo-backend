using todoapp_backend;

namespace todoapp_backend.Services
{
    public interface ITokenService
    {
        string CreateToken(User user);
    }
}
