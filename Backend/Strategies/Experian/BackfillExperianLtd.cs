namespace EzBob.Backend.Strategies.Experian {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillExperianLtd : AStrategy {
		#region public

		#region constructor

		public BackfillExperianLtd(AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BackfillExperianLtd"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			IEnumerable<SafeReader> lst = DB.ExecuteEnumerable("LoadServiceLogForLtdBackfill", CommandSpecies.StoredProcedure);

			foreach (SafeReader sr in lst)
				new ParseExperianLtd(sr["Id"], DB, Log).Execute();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private
		#endregion private
	} // class BackfillExperianLtd
} // namespace
