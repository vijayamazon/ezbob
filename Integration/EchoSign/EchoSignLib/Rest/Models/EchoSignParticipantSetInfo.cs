namespace EchoSignLib.Rest.Models {
    using EchoSignLib.Rest.Models.Enums;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    internal class EchoSignParticipantSetInfo {
        [JsonConverter(typeof(StringEnumConverter))]
        public EchoSignUserAgreementStatus status { get; set; }
        public EchoSignParticipantInfo[] participantSetMemberInfos { get; set; }
    }
}
