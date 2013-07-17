namespace EzBob.TeraPeakServiceLib.Stub
{
    public class ConnectionProviderStub : ITeraPeakConnectionProvider
    {
        public string Url
        {
            get { return "http://api.terapeak.com/v1/research/xml/restricted"; }
        }
    }
}