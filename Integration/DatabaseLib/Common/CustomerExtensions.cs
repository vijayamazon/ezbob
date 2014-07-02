namespace EZBob.DatabaseLib.Common {
	using EZBob.DatabaseLib;
	using EZBob.DatabaseLib.Model.Database;

	public static class CustomerExtensions {
		public static ExperianParserOutput ParseExperian(this Customer customer, ExperianParserFacade.Target nTarget) {
			if (ReferenceEquals(customer.Company, null))
				return new ExperianParserOutput(null, Ezbob.ExperianParser.ParsingResult.NotFound, "Customer has no company", null, null, null);

			return ExperianParserFacade.Invoke(
				customer.Company.ExperianRefNum,
				customer.Company.ExperianCompanyName,
				nTarget,
				customer.Company.TypeOfBusiness.Reduce()
			);
		} // ParseExperian
	} // class CustomerExtensions
} // namespace
