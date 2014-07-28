namespace EzBobTest
{
	using System;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Utils.dbutils;
	using NUnit.Framework;

	[TestFixture]
	public class TestClassToDb
	{
		[Test]
		public void TestClassToDbTableCreate()
		{
			Console.WriteLine(CodeToSql.GetCreateTable<ExperianConsumerData>());
			Console.WriteLine(CodeToSql.GetCreateSp<ExperianConsumerData>());
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
