using System;
using System.IO;
using System.Threading;
using System.Xml.Serialization;

namespace AmazonToken
{
    class XMLLineHandler
    {
        public string id;
        public string outputInfo;
        public int throttledRetries;

        private AuthTokenRetriver getAuthToken = new AuthTokenRetriver();
        XmlSerializer serializer = new XmlSerializer(typeof(AmazonSecurityInfo));

        public XMLLineHandler(string id, int throttledRetries = 1)
        {
            this.id = id;
            outputInfo = "";
            this.throttledRetries = throttledRetries;
        }

        public string ReadXMLLine(string inputInfo)
        {
            AmazonSecurityInfo info = MakeDeSerialized(inputInfo);
            if (info == null)
                return "--XML input is wrong. Entry id=" + id;

            int i = 0;
            for (; i < throttledRetries && getAuthToken.doRetry; i++)
            {
                getAuthToken = getAuthToken.GetAuthToken(info.MerchantId);
                if (getAuthToken.doRetry)
                {
                    Console.WriteLine("id: " + id + " :Request is throttled, trying again in 60 seconds...(Retry #" + (i + 1) + ")");
                    Thread.Sleep(60 * 1000);
                }
            }

            if (getAuthToken.doRetry)
                return "--ERROR: Request is throttled after " + (i + 2).ToString() + " attempts. Entry id=" + id;

            if (getAuthToken.doRetry == false && getAuthToken.MWSAuthToken == "")
                return "--ERROR: Cannot request MWSAuthToken for own account. Entry id=" + id;

            info.MWSAuthToken = getAuthToken.MWSAuthToken;
            return "UPDATE MP_CustomerMarketPlace SET SecurityData=cast('" + MakeSerialized(info) + "' as varbinary(max)) WHERE Id='" + id + "' GO"; //print handler
        }//Getting SecurityData in string form and returns an sql-UPDATE string of a single line
        private string MakeSerialized(AmazonSecurityInfo data)
        {
            StringWriter writer = new StringWriter();
            serializer.Serialize(writer, data);
            return writer.ToString();
        }

        private AmazonSecurityInfo MakeDeSerialized(string data)
        {
            try
            {
                StringReader reader = new StringReader(data);
                return (AmazonSecurityInfo)serializer.Deserialize(reader);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
