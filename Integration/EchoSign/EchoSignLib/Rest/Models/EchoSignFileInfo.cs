namespace EchoSignLib.Rest.Models {
    internal class EchoSignFileInfo {
        public string transientDocumentId { get; set; }
        public string libraryDocumentId { get; set; }
        public string libraryDocumentName { get; set; }
        public EchoSignUrlFileInfo documentURL { get; set; }
    }
}