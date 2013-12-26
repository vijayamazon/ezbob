namespace EzService {
	sealed class SafeValue<T> {
		#region public

		#region constructor

		public SafeValue() {
			m_oLock = new object();

			Value = default(T);
		} // constructor

		public SafeValue(T oValue) {
			m_oLock = new object();

			Value = oValue;
		} // constructor

		#endregion constructor

		#region property Value

		public T Value {
			get {
				lock (m_oLock)
					return m_oValue;
			} // get
			set {
				lock (m_oLock)
					m_oValue = value;
			} // set
		} // Value

		private T m_oValue;

		#endregion property Value

		#endregion public

		#region private

		private readonly object m_oLock;

		#endregion private
	} // class SafeValue
} // namespace EzService
