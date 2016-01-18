namespace EzbobAgreementsHistory {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using Ezbob.Database;
	using Ezbob.Logger;

	class Program {
		static void Main(string[] args) {
			var app = new Program();

			app.Init(args);
			app.Run();
		} // main

		static Program() {
			releaseDates = new SortedSet<DateTime>(ReleaseDateList.Split(',').Select(Agreement.S2D)); 
		} // static constructor

		private Program() {
			this.log = new FileLog(Assembly.GetExecutingAssembly().GetName().Name);
			Ezbob.Context.Environment env = new Ezbob.Context.Environment(this.log);
			this.db = new SqlConnection(env, this.log);

			this.agreements = new List<Agreement>();
		} // constructor

		private void Init(string[] args) {
			this.targetDirectory = args.Length > 0 ? args[0] : ".";

			if (!Directory.Exists(this.targetDirectory))
				this.targetDirectory = ".";

			this.log.Msg("Target directory: {0}", this.targetDirectory);

			Agreement.InitBasePaths(this.db, this.log);

			foreach (var pair in agreementChangeDates)
				this.agreements.Add(new Agreement(pair.Key, pair.Value, releaseDates, this.db, this.log));
		} // Init

		private void Run() {
			this.log.Info("Release dates: {0}", string.Join(" ", releaseDates.Select(Agreement.D2S)));

			foreach (var agreement in this.agreements)
				agreement.Log().CopyFiles(this.targetDirectory);
		} // Run

		private readonly List<Agreement> agreements;
		private readonly AConnection db;
		private readonly ASafeLog log;

		private string targetDirectory;

		private static readonly SortedSet<DateTime> releaseDates; 

		private const string ReleaseDateList = "02/04/2013,10/05/2013,31/05/2013,30/06/2013,28/07/2013,01/09/2013,13/10/2013,01/12/2013,01/02/2014,02/02/2014,03/02/2014,07/12/2014,18/02/2015,11/03/2015,15/03/2015,30/03/2015,31/03/2015,30/04/2015,03/05/2015,10/05/2015,26/05/2015,07/06/2015,21/06/2015,15/07/2015,16/07/2015,26/07/2015,18/08/2015,30/08/2015,01/09/2015,18/10/2015,08/11/2015,29/11/2015";

		private static readonly SortedDictionary<string, string> agreementChangeDates = new SortedDictionary<string, string> {
			{ "GuarantyAgreement", "02/04/2013,18/06/2013,10/03/2014,11/05/2014,28/10/2014,07/01/2015,24/02/2015,01/04/2015,12/04/2015,24/09/2015,01/10/2015,12/10/2015,22/10/2015" },
			{ "PreContractAgreement", "02/04/2013,18/06/2013,19/06/2013,20/06/2013,24/06/2013,25/06/2013,30/06/2013,09/01/2014,19/01/2014,27/03/2014,11/05/2014,24/02/2015,12/04/2015,24/08/2015,25/08/2015,24/09/2015,11/10/2015,12/10/2015,22/10/2015,10/11/2015,22/11/2015" },
			{ "PrivateCompanyLoanAgreement", "02/04/2013,18/06/2013,19/06/2013,20/06/2013,23/10/2013,27/10/2013,13/11/2013,25/11/2013,09/01/2014,19/01/2014,10/03/2014,11/05/2014,28/10/2014,29/10/2014,30/10/2014,01/01/2015,03/02/2015,18/02/2015,24/02/2015,01/04/2015,12/04/2015,09/08/2015,24/08/2015,24/09/2015,12/10/2015" },
			{ "CreditActAgreement", "02/04/2013,18/06/2013,19/06/2013,20/06/2013,24/06/2013,25/06/2013,30/06/2013,27/10/2013,13/11/2013,10/03/2014,27/03/2014,11/05/2014,12/05/2014,01/01/2015,24/02/2015,01/04/2015,12/04/2015,09/08/2015,24/08/2015,25/08/2015,24/09/2015,29/09/2015,12/10/2015,22/10/2015,10/11/2015,22/11/2015" },
			// { "DirectorsConsentDeclaration", "26/01/2014,29/01/2014,24/09/2015" },
			// { "BetaTesterCompanyLoan", "02/04/2013" },
			// { "BetaTesterPreContract", "02/04/2013" },
		};
	} // class Program
} // namespace
