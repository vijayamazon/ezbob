namespace Ezbob.Utils {
	using System;
	using System.Collections;
	using System.Collections.Generic;

	public enum ElapsedDataMemberType {
		RetrieveDataFromExternalService,
		RetrieveDataFromDatabase,
		StoreDataToDatabase,
		AggregateData,
		StoreAggregatedData
	} // enum ElapsedDataMemberType

	public class ElapsedTimeInfo : IEnumerable<ElapsedDataMemberType> {
		private readonly SortedDictionary<ElapsedDataMemberType, long> m_oData;

		public ElapsedTimeInfo() {
			m_oData = new SortedDictionary<ElapsedDataMemberType, long>();

			foreach (ElapsedDataMemberType type in Enum.GetValues(typeof(ElapsedDataMemberType)))
				m_oData.Add(type, 0);
		} // constructor

		public void MergeData(ElapsedTimeInfo other) {
			foreach (var key in other)
				IncreateData(key, other.GetValue(key));
		} // MergeData

		public void IncreateData(ElapsedDataMemberType type, long value) {
			m_oData[type] += value;
		} // IncreateData

		public long GetValue(ElapsedDataMemberType type) {
			return m_oData[type];
		} // GetValue

		public IEnumerator<ElapsedDataMemberType> GetEnumerator() {
			return m_oData.Keys.GetEnumerator();
		} // GetEnumerator

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		} // GetEnumerator
	} // class ElapsedTimeInfo
} // namespace
