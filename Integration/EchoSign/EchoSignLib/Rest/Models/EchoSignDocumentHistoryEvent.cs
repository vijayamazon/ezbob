﻿using System;

namespace EchoSignLib.Rest.Models {
    using EchoSignLib.Rest.Models.Enums;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    internal class EchoSignDocumentHistoryEvent {
        public string vaultEventId { get; set; }
        public string participantEmail { get; set; }
        public string synchronizationId { get; set; }
        public string description { get; set; }
        public string versionId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public EchoSignAgreementEventType type { get; set; }
        public DateTime date { get; set; }
        public string comment { get; set; }
        public string actingUserIpAddress { get; set; }
        public EchoSignDeviceLocation deviceLocation { get; set; }
        public string actingUserEmail { get; set; }
    }
}
