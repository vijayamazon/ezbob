namespace EzBob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class LoadCustomerLeadFieldNames : AStrategy {
		#region public

		#region constructor

		public LoadCustomerLeadFieldNames(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Result = new SortedDictionary<string, string>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "LoadCustomerLeadFieldNames"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Result.Clear();

			DB.ForEachRowSafe(
				sr => { Result[sr["UiControlName"]] = sr["LeadDatumFieldName"]; },
				"LoadCustomerLeadFieldNames",
				CommandSpecies.StoredProcedure
			);
		} // Execute

		#endregion method Execute

		#region property Result

		public SortedDictionary<string, string> Result { get; private set; }

		#endregion property Result

		#endregion public

		#region private

		#endregion private
	} // class LoadCustomerLeadFieldNames
} // namespace
