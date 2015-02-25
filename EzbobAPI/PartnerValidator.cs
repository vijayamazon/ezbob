using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzbobAPI {
	using System.IdentityModel.Selectors;
	using System.ServiceModel;

	public class PartnerValidator : UserNamePasswordValidator {
		/// <summary>
		/// When overridden in a derived class, validates the specified username and password.
		/// </summary>
		/// <param name="userName">The username to validate.</param><param name="password">The password to validate.</param>
		public override void Validate(string userName, string password) {

			Console.WriteLine("======================={0}, {1}", userName, password);

			if (null == userName || null == password) {
				throw new ArgumentNullException();
			}

			/*if (!(userName == "test1" && password == "1tset") && !(userName == "test2" && password == "2tset")) {
				// This throws an informative fault to the client.
				throw new FaultException("Unknown Username or Incorrect Password");
				// When you do not want to throw an infomative fault to the client,
				// throw the following exception.
				// throw new SecurityTokenException("Unknown Username or Incorrect Password");
			}*/
		}
	}
}