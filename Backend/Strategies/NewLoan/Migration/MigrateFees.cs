﻿namespace Ezbob.Backend.Strategies.NewLoan.Migration {
	public class MigrateFees : AStrategy {

		//public MigrateFees() {}

		public override string Name { get { return "MigrateFees"; } }
		public string Error { get; private set; }


		public override void Execute() {

			const string query = "";

			NL_AddLog(LogType.Info, "Started", query, null, null, null);


			NL_AddLog(LogType.Info, "Ended", null, null, null, null);

		} //Execute


		


	
	}
}//ns