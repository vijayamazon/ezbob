namespace EzServiceCrontab.ArgumentTypes {
	internal class LongInt : AType<long> {
		public LongInt() : base("long") {}

		public override object CreateInstance(string sValue) {
			long nResult;

			if (long.TryParse(sValue, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

	} // class LongInt
} // namespace
