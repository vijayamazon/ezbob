namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using Exceptions;

	public class Cache<TKey, TValue> where TValue : class {

		public Cache(TimeSpan oAge, Func<TKey, TValue> oUpdater) {
			m_oData = new SortedDictionary<TKey, StoredValue<TValue>>();
			m_oDataLock = new object();

			m_oAge = oAge;
			m_oValueUpdater = oUpdater;
		} // constructor

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

		private TValue SetOrUpdate(TKey sKey, TValue oValue) {
			lock (m_oDataLock) {
				if (m_oData.ContainsKey(sKey))
					m_oData[sKey].Update(oValue);
				else
					m_oData[sKey] = new StoredValue<TValue>(oValue);
			} // lock

			return oValue;
		} // SetOrUpdate

		private TValue UpdateValue(TKey idx) {
			if (m_oValueUpdater == null)
				throw new NullSeldenException("Cache value updater not specified.");

			return m_oValueUpdater(idx);
		} // UpdateValue

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

		private readonly SortedDictionary<TKey, StoredValue<TValue>> m_oData;
		private readonly object m_oDataLock;
		private readonly TimeSpan m_oAge;
		private readonly Func<TKey, TValue> m_oValueUpdater;

	} // class Cache
} // namespace
