using System.Text;

namespace EchoSignLib.Rest
{
    using System.Net.Http;

    /// <summary>
    /// Json content to use with <see cref="HttpClient"/>.
    /// </summary>
    public class JsonContent : StringContent
    {
        public JsonContent(string json)
            : base(json, Encoding.UTF8, "application/json") { }
    }
}
