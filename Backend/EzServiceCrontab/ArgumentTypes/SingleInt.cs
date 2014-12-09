namespace EzServiceCrontab.ArgumentTypes {
	internal class SingleInt : AType<int> {
		public SingleInt() : base("int") {}

		public override object CreateInstance(string sValue) {
			int nResult;

			if (int.TryParse(sValue, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

	} // class SingleInt
} // namespace
