namespace Ezbob.Utils {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Runtime.Serialization;

	[DataContract]
	public enum ElapsedDataMemberType {
		[EnumMember]
		RetrieveDataFromExternalService,

		[EnumMember]
		RetrieveDataFromDatabase,

		[EnumMember]
		StoreDataToDatabase,

		[EnumMember]
		AggregateData,

		[EnumMember]
		StoreAggregatedData
	} // enum ElapsedDataMemberType

	[DataContract]
	public class ElapsedTimeInfo : IEnumerable<ElapsedDataMemberType> {
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

		[DataMember]
		private readonly SortedDictionary<ElapsedDataMemberType, long> m_oData;
	} // class ElapsedTimeInfo

} // namespace
