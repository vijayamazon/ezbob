namespace Ezbob.Dabinuto {
	class Counter {
		public Counter() {
			Directories = 0;
			Files = new FileCounter();
		} // constructor

		public int Directories { get; set; }

		public FileCounter Files { get; private set; }

		public class FileCounter {
			public FileCounter() {
				Skipped = 0;
				Executed = 0;
			} // constructor

			public int Skipped { get; set; }
			public int Executed { get; set; }

			public int Processed {
				get { return Skipped + Executed; }
			} // Processed
		} // class FileCounter
	} // class Counter
} // namespace
