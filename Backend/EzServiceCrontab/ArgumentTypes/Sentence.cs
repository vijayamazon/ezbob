namespace EzServiceCrontab.ArgumentTypes {
	internal class Sentence : AType<string> {
		public Sentence() : base("string") {}

		public override string FullName {
			get { return Name; }
		} // FullName

		public override object CreateInstance(string sValue) {
			return sValue;
		} // CreateInstance

	} // class Sentence
} // namespace
