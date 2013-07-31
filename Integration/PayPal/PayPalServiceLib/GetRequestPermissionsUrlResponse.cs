namespace EzBob.PayPalServiceLib
{
    public class GetRequestPermissionsUrlResponse
    {
        private readonly string _url;
        private readonly string _token;

        public GetRequestPermissionsUrlResponse(string url, string token)
        {
            _url = url;
            _token = token;
        }

        public string Url
        {
            get { return _url; }
        }

        public string Token
        {
            get { return _token; }
        }
    }
}