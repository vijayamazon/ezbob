namespace EzBob.Backend.Strategies
{
	using Ezbob.Database;
	using Ezbob.Logger;
	using System.Data;
	using System.IO;
	using System.Text;
	using Models;

	public class CaisUpdate : AStrategy
	{
		public CaisUpdate(int caisId, AConnection oDB, ASafeLog oLog)
			: base(oDB, oLog)
		{
			this.caisId = caisId;
		}

		public override string Name {
			get { return "CAIS Generator"; }
		} // Name
		
		public void Execute() {
			DataTable dt = DB.ExecuteReader("GetCaisFileData", CommandSpecies.StoredProcedure);
			DataRow results = dt.Rows[0];

			string fileName = results["FileName"].ToString();
			string dirName = results["DirName"].ToString();

			var unzippedFileContent = strategyHelper.GetCAISFileById(caisId);
			File.WriteAllText(string.Format("{0}\\{1}", dirName, fileName), unzippedFileContent, Encoding.ASCII);
		}

		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly int caisId;
	} // CaisGenerator
} // namespace
