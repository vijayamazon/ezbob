namespace EzServiceCrontab.ArgumentTypes {
	internal class Money : AType<decimal> {
		public Money() : base("decimal") {}

		public override object CreateInstance(string sValue) {
			decimal nResult;

			if (decimal.TryParse(sValue, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

	} // class Money
} // namespace
