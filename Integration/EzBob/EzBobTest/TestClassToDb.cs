﻿namespace EzBobTest
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
			Console.WriteLine(CodeToSql.GetCreateTable<BrokerInstantOfferRequest>());
			Console.WriteLine(CodeToSql.GetCreateSp<BrokerInstantOfferRequest>());
			Console.WriteLine(CodeToSql.GetCreateTable<BrokerInstantOfferResponse>());
			Console.WriteLine(CodeToSql.GetCreateSp<BrokerInstantOfferResponse>());
			
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
