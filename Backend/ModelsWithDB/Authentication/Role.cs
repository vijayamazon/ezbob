namespace Ezbob.Backend.ModelsWithDB.Authentication {
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Ezbob.Utils;
    public class Role {
         [DataMember]
        public int RoleId { get; set; }
         [DataMember]
        public string Name { get; set; }
         [DataMember]
        public string Description { get; set; }
        [NonTraversable]
        public  List<Permission> Premissions { get; set; }
    }
}