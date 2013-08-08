using Ezbob.HmrcCrawler;
using Ezbob.Logger;

namespace TestCrawler {
	class Program {
		static void Main(string[] args) {
			var crawler = new Crawler("829144784260", "18june1974", new ConsoleLog(new LegacyLog()));

			if (crawler.Init())
				crawler.Run();

			crawler.Done();
		} // Main
	} // class Program
} // namespace TestCrawler
