namespace Ezbob.Backend.Models.ExternalAPI
{
    using System.Runtime.Serialization;

    [DataContract]
    public class AlibabaSaleContractResult
    {
        [DataMember(EmitDefaultValue = true)]
        public long? aliMemberId { get; set; }

        [DataMember(EmitDefaultValue = true)]
        public int? aId { get; set; }
    }
}
