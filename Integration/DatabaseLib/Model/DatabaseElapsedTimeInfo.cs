using EzBob.CommonLib;

namespace EZBob.DatabaseLib.Model
{
	public class DatabaseElapsedTimeInfo
	{
		public DatabaseElapsedTimeInfo()
		{
		}

		public DatabaseElapsedTimeInfo(ElapsedTimeInfo info)
		{
			if(info == null)
			{
				return;
			}

			AggregateData = info.GetValue( ElapsedDataMemberType.AggregateData );
			RetrieveDataFromDatabase = info.GetValue( ElapsedDataMemberType.RetrieveDataFromDatabase );
			RetrieveDataFromExternalService = info.GetValue( ElapsedDataMemberType.RetrieveDataFromExternalService );
			StoreAggregatedData = info.GetValue( ElapsedDataMemberType.StoreAggregatedData );
			StoreDataToDatabase = info.GetValue( ElapsedDataMemberType.StoreDataToDatabase );
		}

		public long StoreDataToDatabase { get; set; }

		public long StoreAggregatedData { get; set; }

		public long RetrieveDataFromExternalService { get; set; }

		public long RetrieveDataFromDatabase { get; set; }

		public long AggregateData { get; set; }
	}
}