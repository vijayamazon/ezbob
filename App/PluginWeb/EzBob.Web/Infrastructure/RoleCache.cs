namespace EzBob.Web.Infrastructure {
	using System;
	using Ezbob.Utils;
	using EzBob.Web.Areas.Broker.Controllers;
	using EZBob.DatabaseLib.Model.Database;
	using ServiceClientProxy;

	public class RoleCache : Cache<RoleCache.UserID, string[]> {
		public RoleCache() : base(TimeSpan.FromMinutes(30), UpdateUserRoles) {} // constructor

		public int GetRoleCount(string userName, CustomerOriginEnum origin) {
			var userID = new UserID {
				UserName = userName,
				OriginID = origin,
			};

			string[] roles = this[userID];

			return (roles == null) ? 0 : roles.Length;
		} // indexer

		public class UserID : IComparable<UserID> {
			public string UserName { get; set; }
			public CustomerOriginEnum OriginID { get; set; }

			/// <summary>
			/// Compares the current object with another object of the same type.
			/// </summary>
			/// <returns>
			/// A value that indicates the relative order of the objects being compared.
			/// The return value has the following meanings:
			/// Value Meaning Less than zero This object is less than the <paramref name="other"/> parameter.
			/// Zero This object is equal to <paramref name="other"/>.
			/// Greater than zero This object is greater than <paramref name="other"/>. 
			/// </returns>
			/// <param name="other">An object to compare with this object.</param>
			public int CompareTo(UserID other) {
				int n = String.Compare(UserName, other.UserName, StringComparison.InvariantCulture);

				return n != 0 ? n : OriginID.CompareTo(other.OriginID);
			} // CompareTo
		} // class UserID

		private static string[] UpdateUserRoles(UserID userID) {
			return new ServiceClient()
				.Instance
				.LoadAllLoginRoles(userID.UserName, userID.OriginID.AsRemote(), false)
				.Records;
		} // UpdateUserRoles
	} // class RoleCache
} // namespace
