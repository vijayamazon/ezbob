using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BankTransactionsParserClient {
	using System.IO;
	using BankTransactionsParser;

	class Program {
		static void Main(string[] args) {
			var parser = new TransactionsParser();

			parser.ParseCsv(GetBytesFromFile(@"c:\ezbob\test-data\bank\bank2.csv"), "bank2.csv");

			parser.ParseCsv(GetBytesFromFile(@"c:\ezbob\test-data\bank\bank3.csv"), "bank3.csv");

			parser.ParseCsv(GetBytesFromFile(@"c:\ezbob\test-data\bank\bank4.csv"), "bank4.csv");
		}

		public static byte[] GetBytesFromFile(string fullFilePath) {
			// this method is limited to 2^32 byte files (4.2 GB)

			FileStream fs = null;
			try {
				fs = File.OpenRead(fullFilePath);
				byte[] bytes = new byte[fs.Length];
				fs.Read(bytes, 0, Convert.ToInt32(fs.Length));
				return bytes;
			} finally {
				if (fs != null) {
					fs.Close();
					fs.Dispose();
				}
			}

		}
	}
}
