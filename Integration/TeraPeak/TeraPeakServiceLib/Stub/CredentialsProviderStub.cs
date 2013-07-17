using EzBob.TeraPeakServiceLib.Requests.ResearchResult;

namespace EzBob.TeraPeakServiceLib.Stub
{
    public class CredentialsProviderStub : ITeraPeakCredentionProvider
    {
        public TeraPeakRequesterCredentials RequesterCredentials
        {
            get
            {
                return IsNewVersionOfCredentials ? null : new TeraPeakRequesterCredentials
                    {
                        Token = "bf69db0c2ad55c207ec9c01f793cf0",
                        UserToken = "5d4f3089986df6cced1e367dda87ee",
                        DeveloperName = "alex_syrotyuk_alex_syrotyuk"
                    };
            }
        }

        public string ApiKey
        {
            get { return "xdz8d8hw4cp5x9napc4s7tpq"; }
        }

        public bool IsNewVersionOfCredentials
        {
            get { return !string.IsNullOrEmpty(ApiKey); }
        }
    }
}