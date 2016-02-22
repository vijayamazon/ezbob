namespace LegalDocs.Code {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Ezbob.Backend.ModelsWithDB.Authentication;
    using ServiceClientProxy;

    public sealed class Session {
        private static volatile Session instance;
        private static readonly object syncRoot = new Object();
        public ServiceClient serviceClient = new ServiceClient();

        private Session() { }

        public List<User> Users { get; set; }

        public User GetUser(string userName, int? OriginID) {

            if (string.IsNullOrEmpty(userName))
                return null;

            if (Users == null)
                Users = new List<User>();

            var user = Users.FirstOrDefault(x => x.UserName == userName && x.OriginID == OriginID);
            if (user == null) {
                user = this.serviceClient.Instance.GetSecurityUser(1,1,userName, OriginID).User;
                Users.Add(user);
            }
            return user;
        }

        public static Session Instance {
            get {
                if (instance == null) {
                    lock (syncRoot) {
                        if (instance == null)
                            instance = new Session();
                    }
                }

                return instance;
            }
        }

    }
}