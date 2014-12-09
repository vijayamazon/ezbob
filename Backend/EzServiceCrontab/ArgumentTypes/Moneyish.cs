namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal class Moneyish : AType<decimal?> {
		public Moneyish() : base("decimal") {}

		public override object CreateInstance(string sValue) {
			decimal nResult;

			if (decimal.TryParse(sValue, out nResult))
				return nResult;

			return Activator.CreateInstance(UnderlyingType);
		} // CreateInstance

	} // class Moneyish
} // namespace
