namespace BankTransactionsParserClient {
	using BankTransactionsParser;

	class Program {
		static void Main(string[] args) {
			var parser = new TransactionsParser();

			parser.ParseFile(@"c:\ezbob\test-data\bank\bank2.csv");
			parser.ParseFile(@"c:\ezbob\test-data\bank\bank3.csv");
			parser.ParseFile(@"c:\ezbob\test-data\bank\bank4.csv");
			parser.ParseFile(@"c:\ezbob\test-data\bank\bank1_2.xls");
			parser.ParseFile(@"c:\ezbob\test-data\bank\bank1_1.xlsx");
			parser.ParseFile(@"c:\ezbob\test-data\bank\Current.July14.csv");
		}
	}
}
