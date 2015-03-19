namespace Ezbob.Backend.Strategies.ExternalAPI {
	using System;
	using EZBob.DatabaseLib.Identity;
	using Newtonsoft.Json;
	using StructureMap;

	public class GetIdentityUser : AStrategy {
		public override string Name {
			get { return "GetIdentityUser"; }
		}

		public GetIdentityUser(string userName) {
			this.Username = userName;
			Result = new IdentityUser();
		}

		public override void Execute() {

			Console.WriteLine(this.Username);

			IdentityUserRepository identityRep = ObjectFactory.GetInstance<IdentityUserRepository>();

			Result = identityRep.ByUsername(this.Username);

			Log.Info("search for username {3}; found: {0}, {1}, {2}", Result.Id, Result.UserName, Result.Claims.Count, this.Username);
		}

		public string Username { get; private set; }
		public IdentityUser Result { get; private set; }
	}
}