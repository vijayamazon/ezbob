namespace Ezbob.Backend.ModelsWithDB.Authentication {
    using System.Runtime.Serialization;

    public class Permission {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string Description { get; set; }
    }
}