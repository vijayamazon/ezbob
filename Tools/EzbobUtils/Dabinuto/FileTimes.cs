namespace Ezbob.Dabinuto {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using Newtonsoft.Json;

	class FileTimes {
		public FileTimes(string envName, string saveToFilePath, bool forceAllScripts) {
			this.envName = envName;
			this.saveToFilePath = saveToFilePath;
			this.forceAllScripts = forceAllScripts;

			if (File.Exists(this.saveToFilePath)) {
				this.lastRunTimes = JsonConvert.DeserializeObject<
					SortedDictionary<string, SortedDictionary<string, DateTime>>
				>(
					File.ReadAllText(this.saveToFilePath, Encoding.UTF8)
				);
			} else 
				this.lastRunTimes = new SortedDictionary<string, SortedDictionary<string, DateTime>>();

			if (!this.lastRunTimes.ContainsKey(this.envName))
				this.lastRunTimes[this.envName] = new SortedDictionary<string, DateTime>();
		} // constructor

		public void Save() {
			File.WriteAllText(
				this.saveToFilePath,
				JsonConvert.SerializeObject(this.lastRunTimes),
				Encoding.UTF8
			);
		} // Save

		public DateTime? this[string filePath] {
			get {
				if (this.forceAllScripts)
					return null;

				var fileTimes = this.lastRunTimes[this.envName];

				return fileTimes.ContainsKey(filePath) ? fileTimes[filePath] : (DateTime?)null;
			} // get
			set {
				if (value != null)
					this.lastRunTimes[this.envName][filePath] = value.Value;
			} // set
		} // indexer

		private readonly SortedDictionary<string, SortedDictionary<string, DateTime>> lastRunTimes;

		private readonly string envName;
		private readonly string saveToFilePath;
		private readonly bool forceAllScripts;
	} // class FileTimes
} // namespace
