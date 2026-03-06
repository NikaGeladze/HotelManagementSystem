using HMS.Domain.Entities;

namespace HMS.Application.Interfaces.Services;

public interface IJwtTokenGenerator
{
    string GenerateToken(ApplicationUser user, IEnumerable<string> roles);
}