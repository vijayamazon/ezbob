namespace EzBobRestTests
{
    using System.Security.Claims;
    using System.Threading.Tasks;
    using Microsoft.Owin.Security.OAuth;

    public class TestOAuth2AuthorizationProvider : OAuthAuthorizationServerProvider
    {
        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
        }

        public override async Task GrantResourceOwnerCredentials
                (OAuthGrantResourceOwnerCredentialsContext context)
        {

            // validate user credentials (demo!)
            // user credentials should be stored securely (salted, iterated, hashed…)

            if (context.UserName != context.Password)
            {
                context.Rejected();
                return;
            }
            // create identity

            var id = new ClaimsIdentity(context.Options.AuthenticationType);

            id.AddClaim(new Claim("Origin", context.UserName));

//            id.AddClaim(new Claim("role", "user"));

            context.Validated(id);
        }
    }
}
