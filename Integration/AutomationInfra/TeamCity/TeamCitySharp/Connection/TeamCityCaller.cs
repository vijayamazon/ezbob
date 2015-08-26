using HttpException = EasyHttp.Infrastructure.HttpException;

namespace TeamCityModels.Connection
{
    using System;
    using System.IO;
    using System.Net;
    using System.Security.Authentication;
    using EasyHttp.Http;
    using TeamCityModels.DomainEntities;

    public class TeamCityCaller : ITeamCityCaller
    {
        private readonly Credentials _configuration = new Credentials();

        public TeamCityCaller(string hostName, bool useSsl)
        {
            if (string.IsNullOrEmpty(hostName))
                throw new ArgumentNullException("hostName");

            this._configuration.UseSSL = useSsl;
            this._configuration.HostName = hostName;
        }

        public void Connect(string userName, string password, bool actAsGuest)
        {
            this._configuration.Password = password;
            this._configuration.UserName = userName;
            this._configuration.ActAsGuest = actAsGuest;
        }

        public T GetFormat<T>(string urlPart, params object[] parts)
        {
            return Get<T>(string.Format(urlPart, parts));
        }

        public void GetFormat(string urlPart, params object[] parts)
        {
            Get(string.Format(urlPart, parts));
        }

        public T PostFormat<T>(object data, string contenttype, string accept, string urlPart, params object[] parts)
        {
            return Post<T>(data.ToString(), contenttype, string.Format(urlPart, parts), accept);
        }

        public void PostFormat(object data, string contenttype, string urlPart, params object[] parts)
        {
            Post(data.ToString(), contenttype, string.Format(urlPart, parts), string.Empty);
        }

        public void PutFormat(object data, string contenttype, string urlPart, params object[] parts)
        {
            Put(data.ToString(), contenttype, string.Format(urlPart, parts), string.Empty);
        }

        public void DeleteFormat(string urlPart, params object[] parts)
        {
            Delete(string.Format(urlPart, parts));
        }

        public void GetDownloadFormat(Action<string> downloadHandler, string urlPart, params object[] parts)
        {
            if (CheckForUserNameAndPassword())
            {
                throw new ArgumentException("If you are not acting as a guest you must supply userName and password");
            }

            urlPart = System.Web.HttpUtility.UrlEncode(urlPart);
            if (string.IsNullOrEmpty(urlPart))
            {
                throw new ArgumentException("Url must be specfied");
            }

            if (downloadHandler == null)
            {
                throw new ArgumentException("A download handler must be specfied.");
            }

            string tempFileName = Path.GetRandomFileName();
            var url = CreateUrl(string.Format(urlPart, parts));

            try
            {
                CreateHttpClient(this._configuration.UserName, this._configuration.Password, HttpContentTypes.ApplicationJson).GetAsFile(url, tempFileName);
                downloadHandler.Invoke(tempFileName);

            }
            finally
            {
                if (System.IO.File.Exists(tempFileName))
                {
                    System.IO.File.Delete(tempFileName);
                }
            }
        }

        public string StartBackup(string urlPart)
        {
            if (CheckForUserNameAndPassword())
                throw new ArgumentException("If you are not acting as a guest you must supply userName and password");

            if (string.IsNullOrEmpty(urlPart))
                throw new ArgumentException("Url must be specfied");

            var url = CreateUrl(urlPart);

            var httpClient = CreateHttpClient(this._configuration.UserName, this._configuration.Password, HttpContentTypes.TextPlain);
            var response = httpClient.Post(url, null, HttpContentTypes.TextPlain);
            ThrowIfHttpError(response, url);

            if (response.StatusCode == HttpStatusCode.OK)
            {
                return response.RawText;
            }

            return string.Empty;
        }

        public T Get<T>(string urlPart)
        {
            var response = GetResponse(urlPart);
            return response.StaticBody<T>();
        }
        
        public void Get(string urlPart)
        {
            GetResponse(urlPart);
        }

        private HttpResponse GetResponse(string urlPart)
        {
            if (CheckForUserNameAndPassword())
                throw new ArgumentException("If you are not acting as a guest you must supply userName and password");

            if (string.IsNullOrEmpty(urlPart))
                throw new ArgumentException("Url must be specfied");

            var url = CreateUrl(urlPart);

            var response = CreateHttpClient(this._configuration.UserName, this._configuration.Password, HttpContentTypes.ApplicationJson).Get(url);
            ThrowIfHttpError(response, url);
            return response;
        }

        public T Post<T>(string data, string contenttype, string urlPart, string accept)
        {
            return Post(data, contenttype, urlPart, accept).StaticBody<T>();
        }

        public bool Authenticate(string urlPart)
        {
            try
            {
                var httpClient = CreateHttpClient(this._configuration.UserName, this._configuration.Password, HttpContentTypes.TextPlain);
                httpClient.ThrowExceptionOnHttpError = true;
                httpClient.Get(CreateUrl(urlPart));

                var response = httpClient.Response;
                return response.StatusCode == HttpStatusCode.OK;
            }
            catch (HttpException exception)
            {
                throw new AuthenticationException(exception.StatusDescription);
            }
        }

        public HttpResponse Post(object data, string contenttype, string urlPart, string accept)
        {
            var client = MakePostRequest(data, contenttype, urlPart, accept);

            return client.Response;
        }

        public HttpResponse Put(object data, string contenttype, string urlPart, string accept)
        {
            var client = MakePutRequest(data, contenttype, urlPart, accept);

            return client.Response;
        }

        public void Delete(string urlPart)
        {
            MakeDeleteRequest(urlPart);
        }

        private void MakeDeleteRequest(string urlPart)
        {
            var client = CreateHttpClient(this._configuration.UserName, this._configuration.Password, HttpContentTypes.TextPlain);
            client.Delete(CreateUrl(urlPart));
            ThrowIfHttpError(client.Response, client.Request.Uri);
        }

        private HttpClient MakePostRequest(object data, string contenttype, string urlPart, string accept)
        {
            var client = CreateHttpClient(this._configuration.UserName, this._configuration.Password, string.IsNullOrWhiteSpace(accept) ? GetContentType(data.ToString()) : accept);

            client.Request.Accept = accept;

            client.Post(CreateUrl(urlPart), data, contenttype);
            ThrowIfHttpError(client.Response, client.Request.Uri);

            return client;
        }

        private HttpClient MakePutRequest(object data, string contenttype, string urlPart, string accept)
        {
            var client = CreateHttpClient(this._configuration.UserName, this._configuration.Password, string.IsNullOrWhiteSpace(accept) ? GetContentType(data.ToString()) : accept);

            client.Request.Accept = accept;

            client.Put(CreateUrl(urlPart), data, contenttype);
            ThrowIfHttpError(client.Response, client.Request.Uri);

            return client;
        }

        private static bool IsHttpError(HttpResponse response)
        {
            var num = (int)response.StatusCode / 100;

            return (num == 4 || num == 5);
        }

        /// <summary>
        /// <para>If the <paramref name="response"/> is OK (see <see cref="IsHttpError"/> for definition), does nothing.</para>
        /// <para>Otherwise, throws an exception which includes also the response raw text.
        /// This would often contain a Java exception dump from the TeamCity REST Plugin, which reveals the cause of some cryptic cases otherwise showing just "Bad Request" in the HTTP error.
        /// Also this comes in handy when TeamCity goes into maintenance, and you get back the banner in HTML instead of your data.</para> 
        /// </summary>
        private static void ThrowIfHttpError(HttpResponse response, string url)
        {
            if(!IsHttpError(response))
                return;
            throw new HttpException(response.StatusCode, string.Format("Error: {0}\nHTTP: {3}\nURL: {1}\n{2}", response.StatusDescription, url, response.RawText, response.StatusCode));
        }

        private string CreateUrl(string urlPart)
        {
            var protocol = this._configuration.UseSSL ? "https://" : "http://";
            var authType = this._configuration.ActAsGuest ? "/guestAuth" : "/httpAuth";

            return string.Format("{0}{1}{2}{3}", protocol, this._configuration.HostName, authType, urlPart);
        }

        private HttpClient CreateHttpClient(string userName, string password, string accept)
        {
            var httpClient = new HttpClient(new TeamcityJsonEncoderDecoderConfiguration());
            httpClient.Request.Accept = accept;
            if (!this._configuration.ActAsGuest)
            {
                httpClient.Request.SetBasicAuthentication(userName, password);
                httpClient.Request.ForceBasicAuth = true;
            }

            return httpClient;
        }

        // only used by the artifact listing methods since i havent found a way to deserialize them into a domain entity
        public string GetRaw(string urlPart)
        {
            if (CheckForUserNameAndPassword())
                throw new ArgumentException("If you are not acting as a guest you must supply userName and password");

            if (string.IsNullOrEmpty(urlPart))
                throw new ArgumentException("Url must be specfied");

            var url = CreateUrl(urlPart);

            var httpClient = CreateHttpClient(this._configuration.UserName, this._configuration.Password, HttpContentTypes.TextPlain);
            var response = httpClient.Get(url);
            if (IsHttpError(response))
            {
                throw new HttpException(response.StatusCode, string.Format("Error {0}: Thrown with URL {1}", response.StatusDescription, url));
            }

            return response.RawText;
        }

        private bool CheckForUserNameAndPassword()
        {
            return !this._configuration.ActAsGuest && string.IsNullOrEmpty(this._configuration.UserName) && string.IsNullOrEmpty(this._configuration.Password);
        }

        private string GetContentType(string data)
        {
            if (data.StartsWith("<"))
                return HttpContentTypes.ApplicationXml;
            return HttpContentTypes.TextPlain;
        }
    }
}
