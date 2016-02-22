namespace EchoSignLib.Rest.Api
{
    using System.Net.Http;
    using System.Text;

    public class EchoSignUploadTransientDocumentRequest {
        private MultipartFormDataContent content = new MultipartFormDataContent();


        /// <summary>
        /// Sets the file to upload.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileContent">Content of the file.</param>
        /// <returns></returns>
        public EchoSignUploadTransientDocumentRequest WithFile(string fileName, byte[] fileContent) {
            this.content.Add(new ByteArrayContent(fileContent), "File", fileName);
            this.content.Add(new StringContent("text/html", Encoding.UTF8), "Mime-Type");
            this.content.Add(new StringContent(fileName, Encoding.UTF8), "File-Name");
            return this;
        }

        /// <summary>
        /// Builds the content.
        /// </summary>
        /// <returns></returns>
        public HttpContent BuildContent() {
            var curContent = this.content;
            this.content = new MultipartFormDataContent();
            return curContent;
        }
    }
}
