namespace Ezbob.Backend.ModelsWithDB.Authentication {
    using System.Runtime.Serialization;
    public class RolePermissionRelation {
        [DataMember]
        public int RoleId { get; set; }
        [DataMember]
        public int PermissionId { get; set; }
    }
}