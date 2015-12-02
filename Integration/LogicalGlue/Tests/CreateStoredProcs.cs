namespace Ezbob.Integration.LogicalGlue.Tests {
	using System;
	using Ezbob.Utils.dbutils;
	using NUnit.Framework;

	[TestFixture]
	class CreateStoredProcs {
		[Test]
		public void CreateResponse() {
			CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.Response>();
		} // CreateResponse

		[Test]
		public void CreateModelOutput() {
			CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.ModelOutput>();
		} // CreateModelOutput

		[Test]
		public void CreateEncodingFailure() {
			CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.EncodingFailure>();
		} // CreateEncodingFailure

		// CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.MissingColumn>();
		// CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.OutputRatio>();
		// CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.Warning>();

		private void CreateSp<T>() where T: class {
			Console.WriteLine("{0}", CodeToSql.GetCreateSp<T>());
		} // CreateSp
	} // class CreateStoredProcs
} // namespace
