namespace Ezbob.Database {
	public abstract class AResultRow : IResultRow {

		public virtual bool IsFirst() {
			return m_bIsFirst;
		} // IsFirst

		public virtual void SetIsFirst(bool bIsFirst) {
			m_bIsFirst = bIsFirst;
		} // SetIsFirst

		protected AResultRow() {
			m_bIsFirst = false;
		} // constructor

		private bool m_bIsFirst;

	} // class AResultRow
} // namespace Ezbob.Database
