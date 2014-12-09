namespace EzService {
	sealed class SafeValue<T> {

		public SafeValue() {
			m_oLock = new object();

			Value = default(T);
		} // constructor

		public SafeValue(T oValue) {
			m_oLock = new object();

			Value = oValue;
		} // constructor

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

		private readonly object m_oLock;

	} // class SafeValue
} // namespace EzService
