using System.Security.Claims;
using EventFoto.Data.Models;
using EventFoto.Tests.TestConstants;
using Microsoft.Identity.Web;

namespace EventFoto.Tests.TestBedSetup;

public class UserClaimsProvider
{
    public IList<Claim> Claims { get; }

    public UserClaimsProvider(IList<Claim> claims)
    {
        Claims = claims;
    }

    public UserClaimsProvider()
    {
        Claims = new List<Claim>();
    }

    public static UserClaimsProvider WithUser(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.Name),
            new(ClaimConstants.ObjectId, user.Id.ToString()),
        };
        if (user.GroupAssignment.HasValue)
        {
            claims.Add(new Claim("groups", user.GroupAssignment.Value.ToString()));
        }
        return new UserClaimsProvider(claims);
    }
}
