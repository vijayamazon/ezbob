namespace EchoSignLib.Rest.Models
{
    using EchoSignLib.EchoSignService;

    class EchoSignDocumentCreateInfo
    {
        public string name { get; set; }
        public string signatureType { get; set; }
        public string reminderFrequency { get; set; }
        public string signatureFlow { get; set; }
        public int daysUntilSigningDeadline { get; set; }
        public EchoSignRecipientSetInfo[] recipientSetInfos { get; set; }
        public EchoSignFileInfo[] fileInfos { get; set; }
    }
}
