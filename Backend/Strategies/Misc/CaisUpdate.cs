namespace Ezbob.Backend.Strategies.Misc {
	using System.IO;
	using System.Text;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using EZBob.DatabaseLib.Repository;
	using StructureMap;

	public class CaisUpdate : AStrategy {
		public CaisUpdate(int caisId) {
			this.caisId = caisId;
			this.caisReportsHistoryRepository = ObjectFactory.GetInstance<CaisReportsHistoryRepository>();
		} // constructor

		public override string Name {
			get { return "CAIS Generator"; }
		} // Name

		public override void Execute() {
			SafeReader sr = DB.GetFirst(
				"GetCaisFileData",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CaisId", caisId)
			);

			string fileName = sr["FileName"];
			string dirName = sr["DirName"];

			var file = caisReportsHistoryRepository.Get(caisId);
			var unzippedFileContent = file != null ? ZipString.Unzip(file.FileData) : "";

			File.WriteAllText(string.Format("{0}\\{1}", dirName, fileName), unzippedFileContent, Encoding.ASCII);
		} // Execute

		private readonly int caisId;
		private readonly CaisReportsHistoryRepository caisReportsHistoryRepository;
	} // CaisGenerator
} // namespace
