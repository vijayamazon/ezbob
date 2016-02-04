namespace Ezbob.Backend.ModelsWithDB.Authentication {
    using System.Runtime.Serialization;
    public class UserRoleRelation {
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public int RoleId { get; set; }
    }
}