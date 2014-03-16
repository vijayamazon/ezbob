namespace Ezbob.Database {
	public abstract class AResultRow : IResultRow {
		#region public

		#region method IsFirst

		public virtual bool IsFirst() {
			return m_bIsFirst;
		} // IsFirst

		#endregion method IsFirst

		#region method SetIsFirst

		public virtual void SetIsFirst(bool bIsFirst) {
			m_bIsFirst = bIsFirst;
		} // SetIsFirst

		#endregion method SetIsFirst

		#region method SetNotFirst

		public virtual void SetNotFirst(bool bNotFirst) {
			SetIsFirst(!bNotFirst);
		} // SetNotFirst

		#endregion method SetNotFirst

		#region method HowToProceed

		public virtual ActionResult HowToProceed() {
			return m_nProceedStrategy;
		} // HowToProceed

		#endregion method HowToProceed

		#region method Execute

		public virtual void Execute() {
			// nothing here, may be overridden
		} // Execute

		#endregion method Execute

		#endregion public

		#region protected

		protected AResultRow(ActionResult nProceedStrategy = ActionResult.Continue) {
			m_bIsFirst = false;
			m_nProceedStrategy = nProceedStrategy;
		} // constructor

		#endregion protected

		#region private

		private bool m_bIsFirst;
		private readonly ActionResult m_nProceedStrategy;

		#endregion private
	} // class AResultRow
} // namespace Ezbob.Database
