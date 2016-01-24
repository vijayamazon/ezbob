namespace EzBobCommon.Web {
    using System.Net.Http;
    using System.Threading.Tasks;

    public interface IEzBobHtmlClient {
        /// <summary>
        /// Downloads the page asynchronously as string.
        /// </summary>
        /// <param name="pageAddress">The page address.</param>
        /// <returns></returns>
        Task<string> DownloadPageAsyncAsString(string pageAddress);

        /// <summary>
        /// Downloads the page asynchronously as byte array.
        /// </summary>
        /// <param name="pageAddress">The page address.</param>
        /// <returns></returns>
        Task<byte[]> DownloadPageAsyncAsByteArray(string pageAddress);

        /// <summary>
        /// Posts asynchronously and get string response.
        /// </summary>
        /// <param name="url">The URL.</param>
        /// <param name="httpContent">Content of the HTTP.</param>
        /// <returns></returns>
        Task<string> PostAsyncAndGetStringResponse(string url, HttpContent httpContent);

        /// <summary>
        /// Posts the asynchronously and deserializes json response to provided DTO.
        /// </summary>
        /// <typeparam name="T">the DTO</typeparam>
        /// <param name="url">The URL.</param>
        /// <param name="httpContent">Content of the HTTP.</param>
        /// <returns></returns>
        Task<T> PostAsyncAndGetJsonReponseAs<T>(string url, HttpContent httpContent) where T : new();

        /// <summary>
        /// Gets json response and converts it to specified T.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url">The URL.</param>
        /// <returns></returns>
        Task<T> GetAsyncJsonResponseAs<T>(string url) where T : class, new();
    }
}