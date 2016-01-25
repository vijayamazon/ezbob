namespace Ezbob.Integration.LogicalGlue.Keeper.Implementation.StoredProcedures {
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.dbutils;

	internal class RemoteRequestsAreEnabled : ALogicalGlueStoredProc {
		public RemoteRequestsAreEnabled(AConnection db, ASafeLog log) : base(db, log) {
		} // constructor

		public override bool HasValidParameters() {
			return true;
		} // HasValidParamters

		public bool Execute() {
			ExecuteNonQuery();

			switch ((Value ?? string.Empty).Trim().ToLowerInvariant()) {
			case "1":
			case "yes":
			case "true":
				return true;

			default:
				return false;
			} // switch
		} // Execute

		[Direction(ParameterDirection.Output)]
		[Length(255)]
		public string Value { get; set; }
	} // class RemoteRequestsAreEnabled
} // namespace
