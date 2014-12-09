using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Context;
using Ezbob.Logger;

namespace ez1091 {
	class Program {
		static void Main(string[] args) {
			const string DataFileName = "ez-1091.csv";

			var log = new FileLog("ez1091", oLog: new ConsoleLog());

			var env = new Ezbob.Context.Environment(Name.Production, oLog: log);

			RequestedTransactionsLibrary oRequested = LoadRequested(DataFileName, log);

			// if (oRequested.Count > 1)

			log.Dispose();
		} // Main

		private static RequestedTransactionsLibrary LoadRequested(string sDataFileName, ASafeLog log) {
			var oResult = new RequestedTransactionsLibrary(log);

			log.Debug("Loading requested transactions from {0}", sDataFileName);

			string[] aryFile = File.ReadAllLines(sDataFileName);

			foreach (var sLine in aryFile) {
				if (string.IsNullOrWhiteSpace(sLine))
					continue;

				oResult.Add(sLine);
			} // for each line

			log.Debug("Loading requested transactions from {0} complete, {1} entries found", sDataFileName, oResult.Count);

			return oResult;
		} // LoadRequested
	} // class Program
} // namespace
