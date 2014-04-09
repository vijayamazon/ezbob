namespace ExperianLib
{
	using System;
	using EzBobIntegration.Web_References.Consumer;
	using EZBob.DatabaseLib.Model;
	using StructureMap;

	public class InteractiveService
    {
	    private readonly string certificateThumb;
	    private readonly string interactiveService;

        public InteractiveService()
		{
			var configurationVariablesRepository = ObjectFactory.GetInstance<ConfigurationVariablesRepository>();

			certificateThumb = configurationVariablesRepository.GetByName("ExperianCertificateThumb");
			interactiveService = configurationVariablesRepository.GetByName("ExperianInteractiveService");
		}

        public OutputRoot GetOutput(Input inputData)
        {
            var service = new AuthToken(certificateThumb, "CertificateAuthentication,IPAuthentication");
            var token = service.GetAuthToken();
            if (token == null) return null;

            var ws = new InteractiveWS();
            var ctx = ws.RequestSoapContext;
            ctx.Security.Tokens.Add(token);
            ctx.Security.MustUnderstand = false;
            ws.ClientCertificates.Add(service.GetCertificate(certificateThumb));
            ws.Url = interactiveService;
            try
            {
                var callResult = ws.Interactive(new Root { Input = inputData });
                return callResult;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
