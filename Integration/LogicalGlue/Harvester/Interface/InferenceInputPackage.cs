namespace Ezbob.Integration.LogicalGlue.Harvester.Interface {
	public class InferenceInputPackage {
		public InferenceInputPackage(InferenceInput input, int companyID) {
			InferenceInput = input;
			CompanyID = companyID;
		} // constructor

		public InferenceInput InferenceInput { get; private set; }
		public int CompanyID { get; private set; }
	} // class InferenceInputPackage
} // namespace
