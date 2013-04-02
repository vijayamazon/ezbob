using EzBob.Configuration;

namespace ZohoCRM
{
    public class ZohoConfigTest : IZohoConfig
    {
        public string Token { get { return "f4113479c0a518841da5276e9dd91d6a"; } }
        public bool Enabled { get { return true; } }
    }
}