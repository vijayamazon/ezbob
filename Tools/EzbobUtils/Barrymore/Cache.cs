namespace Ezbob.Utils {
	using System;
	using System.Collections.Generic;
	using Exceptions;

	public class Cache<TKey, TValue> where TValue : class {
		public Cache(TimeSpan oAge, Func<TKey, TValue> oUpdater) {
			this.data = new SortedDictionary<TKey, StoredValue<TValue>>();
			this.dataLock = new object();

			this.age = oAge;
			this.valueUpdater = oUpdater;
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

		protected delegate void Retrieving(TKey key, bool foundInCache, bool isTooOld);
		protected delegate void Saving(TKey key, bool isNew);
		protected delegate void Updating(TKey key);

		protected event Retrieving OnRetrieve;
		protected event Saving OnSave;
		protected event Updating OnUpdate;

		private TValue Retrieve(TKey sKey, out bool bContains) {
			bContains = false;
			TValue oResult = null;
			bool foundInCache = false;

			lock (this.dataLock) {
				if (this.data.ContainsKey(sKey)) {
					StoredValue<TValue> oValue = this.data[sKey];
					foundInCache = true;

					if (!oValue.IsTooOld(this.age)) {
						oResult = oValue.Value;
						bContains = true;
					} // if
				} // if
			} // lock

			if (OnRetrieve != null)
				OnRetrieve(sKey, foundInCache, foundInCache && !bContains);

			return oResult;
		} // Retrieve

		private TValue SetOrUpdate(TKey sKey, TValue oValue) {
			bool isNew;

			lock (this.dataLock) {
				if (this.data.ContainsKey(sKey)) {
					this.data[sKey].Update(oValue);
					isNew = false;
				} else {
					this.data[sKey] = new StoredValue<TValue>(oValue);
					isNew = true;
				} // if
			} // lock

			if (OnSave != null)
				OnSave(sKey, isNew);

			return oValue;
		} // SetOrUpdate

		private TValue UpdateValue(TKey idx) {
			if (this.valueUpdater == null)
				throw new NullSeldenException("Cache value updater not specified.");

			if (OnUpdate != null)
				OnUpdate(idx);

			return this.valueUpdater(idx);
		} // UpdateValue

		private class StoredValue<T> where T : class {
			public T Value { get; private set; }

			public StoredValue(T val) {
				Update(val);
			} // constructor

			public void Update(T val) {
				StoredTime = DateTime.UtcNow;
				Value = val;
			} // Update

			public bool IsTooOld(TimeSpan oAge) {
				return StoredTime + oAge <= DateTime.UtcNow;
			} // IsTooOld

			private DateTime StoredTime { get; set; }
		} // StoredValue

		private readonly SortedDictionary<TKey, StoredValue<TValue>> data;
		private readonly object dataLock;
		private readonly TimeSpan age;
		private readonly Func<TKey, TValue> valueUpdater;
	} // class Cache
} // namespace
