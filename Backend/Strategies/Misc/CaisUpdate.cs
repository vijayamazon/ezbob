namespace Ezbob.Backend.Strategies.Misc {
	using EzBob.Models;
	using Ezbob.Database;
	using System.IO;
	using System.Text;

	public class CaisUpdate : AStrategy {
		public CaisUpdate(int caisId) {
			this.caisId = caisId;
		}

		public override string Name {
			get { return "CAIS Generator"; }
		} // Name

		public override void Execute() {
			SafeReader sr = DB.GetFirst("GetCaisFileData", CommandSpecies.StoredProcedure, new QueryParameter("CaisId", caisId));
			string fileName = sr["FileName"];
			string dirName = sr["DirName"];

			var unzippedFileContent = strategyHelper.GetCAISFileById(caisId);
			File.WriteAllText(string.Format("{0}\\{1}", dirName, fileName), unzippedFileContent, Encoding.ASCII);
		}

		private readonly StrategyHelper strategyHelper = new StrategyHelper();
		private readonly int caisId;
	} // CaisGenerator
} // namespace
