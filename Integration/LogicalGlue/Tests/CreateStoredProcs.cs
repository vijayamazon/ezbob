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

		[Test]
		public void CreateMissingColumn() {
			CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.MissingColumn>();
		} // CreateMissingColumn

		[Test]
		public void CreateOutputRatio() {
			CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.OutputRatio>();
		} // CreateOutputRatio

		[Test]
		public void CreateWarning() {
			CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.Warning>();
		} // CreateWarning

		[Test]
		public void CreateEtlData() {
			CreateSp<Ezbob.Integration.LogicalGlue.Keeper.Implementation.DBTable.EtlData>();
		} // CreateEtlData

		private void CreateSp<T>() where T: class {
			Console.WriteLine("{0}", CodeToSql.GetCreateSp<T>());
		} // CreateSp
	} // class CreateStoredProcs
} // namespace
