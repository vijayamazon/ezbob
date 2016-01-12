using System.Text;

namespace EzBobCommon.Http {
    using System.Net.Http;

    /// <summary>
    /// Helps to make post json requests. Can be used for example with <see cref="HttpClient"/>
    /// </summary>
    public class JsonContent : StringContent {
        public JsonContent(string json)
            : base(json, Encoding.UTF8, "application/json") {}
    }
}
