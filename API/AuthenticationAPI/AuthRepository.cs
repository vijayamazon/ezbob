namespace Ezbob.API.AuthenticationAPI
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Threading.Tasks;
	using Ezbob.API.AuthenticationAPI.Entities;
	using Ezbob.API.AuthenticationAPI.Models;
	using Microsoft.AspNet.Identity;
	using Microsoft.AspNet.Identity.EntityFramework;

	public class AuthRepository : IDisposable
    {
        private AuthContext _ctx;

        private UserManager<IdentityUser> _userManager;

		public UserManager<IdentityUser> UserManager;

        public AuthRepository()
        {
            this._ctx = new AuthContext();
            this._userManager = new UserManager<IdentityUser>(new UserStore<IdentityUser>(this._ctx));

			this.UserManager = this._userManager;
        }

        public async Task<IdentityResult> RegisterUser(UserModel userModel)
        {
            IdentityUser user = new IdentityUser
            {
                UserName = userModel.UserName
            };

            var result = await this._userManager.CreateAsync(user, userModel.Password);

            return result;
        }

        public async Task<IdentityUser> FindUser(string userName, string password)
        {
            IdentityUser user = await this._userManager.FindAsync(userName, password);

            return user;
        }

        public Client FindClient(string clientId)
        {
            var client = this._ctx.Clients.Find(clientId);

            return client;
        }

        public async Task<bool> AddRefreshToken(RefreshToken token)
        {
		//	Trace.WriteLine(string.Format("\n\n\n\n\n1----------AddRefreshToken----token {0} ", JsonConvert.SerializeObject(token, Formatting.None, new JsonSerializerSettings {Formatting = Newtonsoft.Json.Formatting.None,ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore})));

           var existingToken = this._ctx.RefreshTokens.Where(r => r.Subject == token.Subject && r.ClientId == token.ClientId).SingleOrDefault();

		   //Trace.WriteLine(string.Format("2----------AddRefreshToken----existingToken {0} ", JsonConvert.SerializeObject(existingToken, Formatting.None, new JsonSerializerSettings {
		   //	Formatting = Newtonsoft.Json.Formatting.None,
		   //	 ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
		   // })));

           if (existingToken != null)
           {
             var result = await RemoveRefreshToken(existingToken);
           }
          
            this._ctx.RefreshTokens.Add(token);

            return await this._ctx.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
           var refreshToken = await this._ctx.RefreshTokens.FindAsync(refreshTokenId);

           if (refreshToken != null) {
               this._ctx.RefreshTokens.Remove(refreshToken);
               return await this._ctx.SaveChangesAsync() > 0;
           }

           return false;
        }

        public async Task<bool> RemoveRefreshToken(RefreshToken refreshToken)
        {
			//Trace.WriteLine(string.Format("1----------RemoveRefreshToken----refreshToken {0} ", JsonConvert.SerializeObject(refreshToken, Formatting.None, new JsonSerializerSettings {
			//	Formatting = Newtonsoft.Json.Formatting.None,
			//	ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
			//})));

            this._ctx.RefreshTokens.Remove(refreshToken);
            return await this._ctx.SaveChangesAsync() > 0;
        }

        public async Task<RefreshToken> FindRefreshToken(string refreshTokenId)
        {
			//Trace.WriteLine(string.Format("1----------FindRefreshToken----refreshTokenId {0} ", refreshTokenId));

            var refreshToken = await this._ctx.RefreshTokens.FindAsync(refreshTokenId);

			//Trace.WriteLine(string.Format("2----------FindRefreshToken----refreshToken {0} ", JsonConvert.SerializeObject(refreshToken, Formatting.None, new JsonSerializerSettings {
			//	Formatting = Newtonsoft.Json.Formatting.None,
			//	ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
			//})));

            return refreshToken;
        }

        public List<RefreshToken> GetAllRefreshTokens()
        {
			//Trace.WriteLine(string.Format("1----------GetAllRefreshTokens----"));

            return  this._ctx.RefreshTokens.ToList();
        }

        public async Task<IdentityUser> FindAsync(UserLoginInfo loginInfo)
        {
            IdentityUser user = await this._userManager.FindAsync(loginInfo);

            return user;
        }

        public async Task<IdentityResult> CreateAsync(IdentityUser user)
        {
            var result = await this._userManager.CreateAsync(user);

            return result;
        }

        public async Task<IdentityResult> AddLoginAsync(string userId, UserLoginInfo login)
        {
            var result = await this._userManager.AddLoginAsync(userId, login);

            return result;
        }


		public async Task<IdentityUser> FindByUserName(string userName) {
			IdentityUser user = await this._userManager.FindByNameAsync(userName);

			return user;
		}


		public async Task<bool> AddToRole(string userID, string role) {

			await this._userManager.AddToRoleAsync(userID, role);

			return await this._ctx.SaveChangesAsync() > 0;
		}

		
        public void Dispose()
        {
            this._ctx.Dispose();
            this._userManager.Dispose();

        }

		public IList<string> GetRoles(string userID) {
			return this._userManager.GetRoles(userID);
		}
	}
}