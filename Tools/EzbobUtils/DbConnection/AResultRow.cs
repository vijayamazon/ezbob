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

		#endregion public

		#region protected

		protected AResultRow() {
			m_bIsFirst = false;
		} // constructor

		#endregion protected

		#region private

		private bool m_bIsFirst;

		#endregion private
	} // class AResultRow
} // namespace Ezbob.Database
