namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal class DoubleFloatish : AType<double?> {
		public DoubleFloatish() : base("double") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			double nResult;

			if (double.TryParse(sValue, out nResult))
				return nResult;

			return Activator.CreateInstance(UnderlyingType);
		} // CreateInstance

		#endregion method CreateInstance
	} // class DoubleFloatish
} // namespace
