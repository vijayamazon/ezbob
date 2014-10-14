namespace EzServiceCrontab.ArgumentTypes {
	internal class Sentence : AType<string> {
		public Sentence() : base("string") {}

		#region property FullName

		public override string FullName {
			get { return Name; }
		} // FullName

		#endregion property FullName

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			return sValue;
		} // CreateInstance

		#endregion method CreateInstance
	} // class Sentence
} // namespace
