namespace Ezbob.API.AuthenticationAPI {
	using System.Data.Entity;
	using Ezbob.API.AuthenticationAPI.Entities;
	using Microsoft.AspNet.Identity.EntityFramework;

	public class AuthContext : IdentityDbContext<IdentityUser> {
		// public AuthContext(): base("AuthContext")   {   }

		public AuthContext()
			: base(new CurrentDbConnection().ConnectionStringName) {
		}

		public DbSet<Client> Clients { get; set; }
		public DbSet<RefreshToken> RefreshTokens { get; set; }
	}
}