using System;
using EzBob.Configuration;
using EzBobIntegration.Web_References.Consumer;

namespace ExperianLib
{
    public class InteractiveService
    {
        readonly ExperianIntegrationParams _config;

        public InteractiveService()
        {
            _config = ConfigurationRootBob.GetConfiguration().Experian;
        }

        public OutputRoot GetOutput(Input inputData)
        {
            var service = new AuthToken(_config.CertificateThumb, "CertificateAuthentication,IPAuthentication");
            var token = service.GetAuthToken();
            if (token == null) return null;

            var ws = new InteractiveWS();
            var ctx = ws.RequestSoapContext;
            ctx.Security.Tokens.Add(token);
            ctx.Security.MustUnderstand = false;
            ws.ClientCertificates.Add(service.GetCertificate(_config.CertificateThumb));
            ws.Url = _config.InteractiveService;
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
