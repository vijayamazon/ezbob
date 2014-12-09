namespace Ezbob.Backend.Strategies.Misc {
	using System.Collections.Generic;
	using Ezbob.Database;

	public class LoadCustomerLeadFieldNames : AStrategy {
		public LoadCustomerLeadFieldNames() {
			Result = new SortedDictionary<string, string>();
		} // constructor

		public override string Name {
			get { return "LoadCustomerLeadFieldNames"; }
		} // Name

		public override void Execute() {
			Result.Clear();

			DB.ForEachRowSafe(
				sr => { Result[sr["UiControlName"]] = sr["LeadDatumFieldName"]; },
				"LoadCustomerLeadFieldNames",
				CommandSpecies.StoredProcedure
			);
		} // Execute

		public SortedDictionary<string, string> Result { get; private set; }
	} // class LoadCustomerLeadFieldNames
} // namespace
