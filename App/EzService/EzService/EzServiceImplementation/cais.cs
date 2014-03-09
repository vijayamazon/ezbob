namespace EzService {
	using EzBob.Backend.Strategies;

	partial class EzServiceImplementation {
		public ActionMetaData CaisGenerate(int underwriterId) {
			return Execute(null, underwriterId, typeof(CaisGenerate), underwriterId);
		} // CaisGenerate

		public ActionMetaData CaisUpdate(int userId, int caisId) {
			return Execute(null, userId, typeof(CaisUpdate), caisId);
		} // CaisUpdate
	} // class EzServiceImplementation
} // namespace EzService
