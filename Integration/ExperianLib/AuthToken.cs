namespace ExperianLib
{
	using System;
	using System.Collections.Generic;
	using System.Security;
	using System.Security.Cryptography.X509Certificates;
	using ConfigManager;
	using EzBobIntegration.Web_References.ExpAuth;
	using EZBob.DatabaseLib.Model;
	using StructureMap;
	using log4net;

	public class AuthToken
	{
		private readonly Dictionary<string, X509Certificate2> _certificateCache;
		private readonly string _certificateToUse;
		private string _url;
		private string _autLevels;
		private static readonly ILog Log = LogManager.GetLogger(typeof(AuthToken));

		public AuthToken(string certificateToUse, string authLevels, string url = null)
		{
			_certificateCache = new Dictionary<string, X509Certificate2>();
			_certificateToUse = certificateToUse;

			_url = url ?? CurrentValues.Instance.ExperianAuthTokenService;

			_autLevels = authLevels;
		}

		//-----------------------------------------------------------------------------------
		public WaspToken GetAuthToken()
		{
			var service = new TokenService { Url = _url };
			var authBlock = string.Format(@"<WASPAuthenticationRequest>
												<ApplicationName>{0}</ApplicationName>
												<AuthenticationLevel>{1}</AuthenticationLevel>
												<AuthenticationParameters/>
											</WASPAuthenticationRequest>", "EzBob - Orange money", _autLevels);

			var certificate = GetCertificate(_certificateToUse);
			service.ClientCertificates.Add(certificate);
			string response = service.STS(authBlock);

			if (response != null && response.IndexOf("Error", StringComparison.Ordinal) < 0)
			{
				return new WaspToken(response);
			}
			Log.Error("GetAuthToken failed, response: " + response);
			return null;
		}

		//-----------------------------------------------------------------------------------
		public X509Certificate2 GetCertificate(string thumb)
		{
			if (_certificateCache.ContainsKey(thumb)) return _certificateCache[thumb];
			var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
			store.Open(OpenFlags.ReadOnly);
			var cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumb, false);
			if (cert.Count == 0)
			{
				store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
				store.Open(OpenFlags.ReadOnly);
				cert = store.Certificates.Find(X509FindType.FindByThumbprint, thumb, false);
			}
			if (cert.Count > 0)
			{
				_certificateCache.Add(thumb, cert[0]);
				return cert[0];
			}
			Log.Error("GetCertificate: cannot find certificate by thumb in locations CurrentUser, LocalMachine: " + thumb);
			throw new Exception("Certificate not found by thumb " + thumb);
		}
	}
}
