namespace Ezbob.Dabinuto {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text;
	using Newtonsoft.Json;

	class FileTimes {
		public FileTimes(string envName, string saveToFilePath) {
			this.envName = envName;
			this.saveToFilePath = saveToFilePath;

			this.lastRunTimes = new SortedDictionary<string, SortedDictionary<string, DateTime>>();
			this.lastRunTimes[this.envName] = new SortedDictionary<string, DateTime>();
		} // constructor

		public void Load() {
			if (!File.Exists(this.saveToFilePath))
				return;

			this.lastRunTimes = JsonConvert.DeserializeObject<SortedDictionary<string, SortedDictionary<string, DateTime>>>(
				File.ReadAllText(this.saveToFilePath, Encoding.UTF8)
			);

			if (!this.lastRunTimes.ContainsKey(this.envName))
				this.lastRunTimes[this.envName] = new SortedDictionary<string, DateTime>();
		} // Load

		public void Save() {
			File.WriteAllText(
				this.saveToFilePath,
				JsonConvert.SerializeObject(this.lastRunTimes),
				Encoding.UTF8
			);
		} // Save

		public DateTime? this[string filePath] {
			get {
				var fileTimes = this.lastRunTimes[this.envName];

				return fileTimes.ContainsKey(filePath) ? fileTimes[filePath] : (DateTime?)null;
			} // get
			set {
				if (value != null)
					this.lastRunTimes[this.envName][filePath] = value.Value;
			} // set
		} // indexer

		private SortedDictionary<string, SortedDictionary<string, DateTime>> lastRunTimes { get; set; }

		private readonly string envName;

		private readonly string saveToFilePath;
	} // class FileTimes
} // namespace
