namespace EzBobTest {
	using System;
	// using Ezbob.Backend.ModelsWithDB;
	using Ezbob.Utils.dbutils;
	using NUnit.Framework;

	[TestFixture]
	public class TestClassToDb {
		[Test]
		public void TestClassToDbTableCreate() {
			// Console.WriteLine(CodeToSql.GetCreateTable<CampaignSourceRef>());
			// Console.WriteLine(CodeToSql.GetCreateSp<CampaignSourceRef>());

			Console.WriteLine(CodeToSql.GetCreateTable<DbTable>());
			Console.WriteLine(CodeToSql.GetCreateTable<OtherTable>());

			Console.WriteLine(CodeToSql.GetCreateSp<DbTable>());
			Console.WriteLine(CodeToSql.GetCreateSp<OtherTable>());
		}
	}

	public class DbTable {
		[PK]
		public long Id { get; set; }

		[FK("ForeignTable", "Id")]
		public long? ForeignId { get; set; }

		[Length(12)]
		public string Field { get; set; }
	}

	public class OtherTable {
		[PK(true)]
		public long Id { get; set; }

		[FK("ForeignTable", "Id")]
		public long? ForeignId { get; set; }

		[Length(-1)]
		public string Field { get; set; }

		[Length(LengthType.D22E2)]
		public decimal Numerical { get; set; }
	}
}
