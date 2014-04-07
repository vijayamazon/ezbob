namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;

	#region class Cache

	public class Cache<TKey, TValue> where TValue : class {
		#region public

		#region constructor

		public Cache(TimeSpan oAge, Func<TKey, TValue> oUpdater) {
			m_oData = new SortedDictionary<TKey, StoredValue<TValue>>();
			m_oDataLock = new object();

			m_oAge = oAge;
			m_oValueUpdater = oUpdater;
		} // constructor

		#endregion constructor

		#region indexer

		public TValue this[TKey idx] {
			get {
				bool bContains = false;
				TValue oValue = Retrieve(idx, out bContains);

				if (bContains)
					return oValue;

				return SetOrUpdate(idx, UpdateValue(idx));
			} // get

			set {
				SetOrUpdate(idx, value);
			} // set
		} // indexer

		#endregion indexer

		#endregion public

		#region private

		#region method Retrieve

		private TValue Retrieve(TKey sKey, out bool bContains) {
			bContains = false;
			TValue oResult = null;

			lock (m_oDataLock) {
				if (m_oData.ContainsKey(sKey)) {
					StoredValue<TValue> oValue = m_oData[sKey];

					if (!oValue.IsTooOld(m_oAge)) {
						oResult = oValue.Value;
						bContains = true;
					} // if
				} // if
			} // lock

			return oResult;
		} // Retrieve

		#endregion method Retrieve

		#region method SetOrUpdate

		private TValue SetOrUpdate(TKey sKey, TValue oValue) {
			lock (m_oDataLock) {
				if (m_oData.ContainsKey(sKey))
					m_oData[sKey].Update(oValue);
				else
					m_oData[sKey] = new StoredValue<TValue>(oValue);
			} // lock

			return oValue;
		} // SetOrUpdate

		#endregion method SetOrUpdate

		#region method UpdateValue

		private TValue UpdateValue(TKey idx) {
			if (m_oValueUpdater == null)
				throw new NullSeldenException("Cache value updater not specified.");

			return m_oValueUpdater(idx);
		} // UpdateValue

		#endregion method UpdateValue

		#region class StoredValue

		private class StoredValue<T> where T : class {
			public T Value { get; private set; }

			public StoredValue(T val) {
				Update(val);
			} // constructor

			public T Update(T val) {
				StoredTime = DateTime.UtcNow;
				Value = val;

				return val;
			} // Update

			public bool IsTooOld(TimeSpan oAge) {
				return StoredTime + oAge >= DateTime.UtcNow;
			} // IsTooOld

			private DateTime StoredTime { get; set; }
		} // StoredValue

		#endregion class StoredValue

		#region fields

		private readonly SortedDictionary<TKey, StoredValue<TValue>> m_oData;
		private readonly object m_oDataLock;
		private readonly TimeSpan m_oAge;
		private readonly Func<TKey, TValue> m_oValueUpdater;

		#endregion fields

		#endregion private
	} // class Cache

	#endregion class Cache
} // namespace Ezbob.Utils
