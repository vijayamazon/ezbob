namespace EzBobTest
{
	using System;
	using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Utils.dbutils;
	using NUnit.Framework;

	[TestFixture]
	public class TestClassToDb
	{
		[Test]
		public void TestClassToDbTableCreate()
		{
			Console.WriteLine(CodeToSql.GetCreateTable<CampaignSourceRef>());
			Console.WriteLine(CodeToSql.GetCreateSp<CampaignSourceRef>());
		}
	}

	public class DbTable
	{
		[PK]
		public long Id { get; set; }
		[FK("ForeignTable", "Id")]
		public long? ForeignId { get; set; }

		public string Field { get; set; }
	}
}
