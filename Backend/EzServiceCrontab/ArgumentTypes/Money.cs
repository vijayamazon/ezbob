namespace EzServiceCrontab.ArgumentTypes {
	internal class Money : AType<decimal> {
		public Money() : base("decimal") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			decimal nResult;

			if (decimal.TryParse(sValue, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

		#endregion method CreateInstance
	} // class Money
} // namespace
