namespace Ezbob.Backend.Strategies.StoredProcs {
	using System;
	using Ezbob.Database;

	internal static class InitCreatePasswordToken {
		public static Guid Execute(AConnection oDB, string sEmail) {
			Guid oToken = Guid.NewGuid();

			bool bSuccess = oDB.ExecuteScalar<bool>(
				"InitCreatePasswordToken",
				CommandSpecies.StoredProcedure,
				new QueryParameter("TokenID", oToken),
				new QueryParameter("Email", sEmail),
				new QueryParameter("Now", DateTime.UtcNow)
			);

			return bSuccess ? oToken : Guid.Empty;
		} // Execute
	} // class InitCreatePasswordToken
} // namespace
