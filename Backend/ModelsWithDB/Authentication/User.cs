namespace Ezbob.Backend.ModelsWithDB.Authentication {
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Ezbob.Utils;

    public class User {
        [DataMember]
        public int UserId { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        public int? OriginID { get; set; }
        [NonTraversable]
        public string SessionId { get; set; }
        [NonTraversable]
        public List<Role> UserRoles { get; set; }
    }
}