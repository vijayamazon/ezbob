namespace Demo.Infrastructure {
	using System.Collections.Generic;
	using Models;

	internal class ValueStorage {
		#region property Instance

		public static ValueStorage Instance {
			get {
				if (ms_oInstance == null)
					ms_oInstance = new ValueStorage();

				return ms_oInstance;
			} // get
		} // Instance

		private static ValueStorage ms_oInstance;

		#endregion property Instance

		public static ValueStorage operator +(ValueStorage oStorage, ValueModel oValue) {
			oValue.ID = ms_nIDGen++;

			ms_oValues[oValue.ID] = oValue;

			return oStorage;
		} // operator +

		public bool IsEmpty {
			get { return ms_oValues.Count < 1; }
		} // IsEmpty

		public IEnumerable<ValueModel> Values {
			get { return ms_oValues.Values; }
		} // Values

		public bool Contains(int nID) {
			return ms_oValues.ContainsKey(nID);
		} // Contains

		public bool Contains(ValueModel oValue) {
			if (oValue == null)
				return false;

			return ms_oValues.ContainsKey(oValue.ID);
		} // Contains

		public ValueModel this[int nID] {
			get { return ms_oValues[nID]; }
		} // indexer

		public bool Update(ValueModel oValue) {
			if (!Contains(oValue.ID))
				return false;

			var oOld = ms_oValues[oValue.ID];
			oOld.Title = oValue.Title;
			oOld.Content = oValue.Content;

			return true;
		} // Update

		public bool Remove(int nID) {
			if (Contains(nID)) {
				ms_oValues.Remove(nID);
				return true;
			} // if

			return false;
		} // Remove

		#region private

		private ValueStorage() {} // constructor

		private static readonly SortedDictionary<int, ValueModel> ms_oValues = new SortedDictionary<int, ValueModel>();
		private static int ms_nIDGen = 1;

		#endregion private
	} // class ValueStorage
} // namespace
