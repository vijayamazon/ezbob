namespace EzService.EzServiceImplementation {
	using EzBob.Backend.Strategies;

	partial class EzServiceImplementation {
		public ActionMetaData CheckExperianCompany(int customerId) {
			return Execute(customerId, null, typeof(ExperianCompanyCheck), customerId);
		} // CheckExperianCompany

		public ActionMetaData CheckExperianConsumer(int customerId, int directorId) {
			return Execute(customerId, null, typeof(ExperianConsumerCheck), customerId, directorId);
		} // CheckExperianConsumer
	} // class EzServiceImplementation
} // namespace EzService
