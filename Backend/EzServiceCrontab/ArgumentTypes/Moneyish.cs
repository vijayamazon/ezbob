namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal class Moneyish : AType<decimal?> {
		public Moneyish() : base("decimal") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			decimal nResult;

			if (decimal.TryParse(sValue, out nResult))
				return nResult;

			return Activator.CreateInstance(UnderlyingType);
		} // CreateInstance

		#endregion method CreateInstance
	} // class Moneyish
} // namespace
