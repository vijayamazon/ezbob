namespace MaamClient {
	using Ezbob.Logger;

	class Args {
		public bool IsGood { get; private set; }
		public int Count { get; private set; }
		public int LastCheckedID { get; private set; }

		public string Query {
			get {
				string top = Count > 0 ? "TOP " + Count : string.Empty;
				string condition = LastCheckedID > 0 ? string.Format(Condition, LastCheckedID) : string.Empty;
				return string.Format(queryFormat, top, condition);
			} // get
			set {
				queryFormat = value;
			}
		} // Query

		public string Condition { private get; set; }

		public Args(string sName, string[] args, ASafeLog oLog) {
			Count = -1;
			LastCheckedID = -1;

			IsGood = false;
			int n;

			if (args.Length < 1) {
				Count = -1;
				LastCheckedID = -1;
				IsGood = true;
				return;
			}

			if (args.Length > 0) {
				IsGood = int.TryParse(args[0], out n);
				if (IsGood)
					Count = n;

				LastCheckedID = -1;
			} // if

			if (IsGood && (args.Length > 1)) {
				IsGood = int.TryParse(args[1], out n);
				if (IsGood)
					LastCheckedID = n;
			} // if

			if (!IsGood) {
				oLog.Msg("Usage: {0} [ <count>  [ <last checked id> ] ]", sName);
				oLog.Msg("Specify count 0 or negative to run on entire data.");
				oLog.Msg("Specify last checked id 0 or negative to start from the beginning.");
			} // if
		} // constructor

		private string queryFormat;
	} // class Args
} // namespace
