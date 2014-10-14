namespace EzServiceCrontab.ArgumentTypes {
	internal class Sentence : AType<string> {
		public Sentence() : base("string") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			return sValue;
		} // CreateInstance

		#endregion method CreateInstance
	} // class Sentence
} // namespace
