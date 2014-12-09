namespace EzService.EzServiceImplementation {
	using Ezbob.Backend.Strategies.Misc;

	partial class EzServiceImplementation {
		public ActionMetaData CaisGenerate(int underwriterId) {
			return Execute<CaisGenerate>(null, underwriterId, underwriterId);
		} // CaisGenerate

		public ActionMetaData CaisUpdate(int userId, int caisId) {
			return Execute<CaisUpdate>(null, userId, caisId);
		} // CaisUpdate
	} // class EzServiceImplementation
} // namespace EzService
