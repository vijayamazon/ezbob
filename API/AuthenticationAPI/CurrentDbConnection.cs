namespace Ezbob.API.AuthenticationAPI {
	using System;

	public class CurrentDbConnection {

		public CurrentDbConnection() {
			//this.ConnectionStringName = "dev-elinar-prod-auto";
			try {
				Context.Environment m_oEnv = new Context.Environment();
				this.ConnectionStringName = m_oEnv.Context.ToLower();
				//Trace.WriteLine("\n\t\t\t**************DB********************" + this.ConnectionStringName);
			} catch (Exception e) {
				throw new NullReferenceException("Failed to determine current environment.", e);
			}
		}

		public string ConnectionStringName { get; private set; }
	}
}