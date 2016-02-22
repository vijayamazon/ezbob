namespace EzBob.Web.Infrastructure {
	using System;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using EZBob.DatabaseLib.Model.Database;
	using ServiceClientProxy;

	public class RoleCache : RoleCacheBase<UserIDForRole> {
		public RoleCache() : base(TimeSpan.FromMinutes(30), UpdateUserRoles) {
		} // constructor

		public int GetRoleCount(string userName, CustomerOriginEnum? origin) {
			var userID = new UserIDForRole {
				UserName = userName,
				OriginID = origin,
			};

			string[] roles = this[userID];

			return (roles == null) ? 0 : roles.Length;
		} // indexer

		private static string[] UpdateUserRoles(UserIDForRole userID) {
			return new ServiceClient()
				.Instance
				.LoadAllLoginRoles(userID.UserName, userID.OriginID, false)
				.Records;
		} // UpdateUserRoles
	} // class RoleCacheBase

	public class RoleCacheBase<TKey> : Cache<TKey, string[]> {
		public RoleCacheBase(TimeSpan oAge, Func<TKey, string[]> oUpdater) : base(oAge, oUpdater) {
			OnRetrieve += LogRetrieving;
			OnSave += LogSaving;
			OnUpdate += LogUpdating;
		} // constructor

		private static void LogRetrieving(TKey userID, bool foundInCache, bool isTooOld) {
			Log().Debug(
				"Retrieve from RoleCache: {0}, {1}found in cache {2}too old.",
				userID,
				foundInCache ? string.Empty : "not ",
				isTooOld ? string.Empty : "not "
			);
		} // LogRetrieving

		private static void LogSaving(TKey userID, bool isNew) {
			Log().Debug(
				"Saving to RoleCache: {0}, {1}.",
				userID,
				isNew ? "saving new" : "updating existing"
			);
		} // LogSaving

		private static void LogUpdating(TKey userID) {
			Log().Debug("Updating from remote source to RoleCache: {0}.", userID);		
		} // LogUpdating

		private static ASafeLog Log() {
			return new SafeILog(typeof(RoleCacheBase<TKey>));
		} // Log
	} // class RoleCacheBase

	public class UserIDForRole : IComparable<UserIDForRole> {
		public string UserName { get; set; }
		public CustomerOriginEnum? OriginID { get; set; }

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
		public int CompareTo(UserIDForRole other) {
			int n = String.Compare(UserName, other.UserName, StringComparison.InvariantCulture);

			if (n != 0)
				return n;

			if ((OriginID == null) && (other.OriginID == null))
				return 0;

			if ((OriginID == null) && (other.OriginID != null))
				return 1;

			if ((OriginID != null) && (other.OriginID == null))
				return -1;

			// ReSharper disable PossibleInvalidOperationException
			// Checking for NULL can safely be disabled because of the three previous "ifs".
			return OriginID.Value.CompareTo(other.OriginID.Value);
			// ReSharper restore PossibleInvalidOperationException
		} // CompareTo

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return String.Format("{0}@{1}", UserName, OriginID.HasValue ? OriginID.Value.ToString() : "-- null --");
		} // ToString
	} // class UserIDForRole
} // namespace
