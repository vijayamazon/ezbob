namespace EzServiceCrontab.ArgumentTypes {
	internal class DoubleFloat : AType<double> {
		public DoubleFloat() : base("double") {}

		public override object CreateInstance(string sValue) {
			double nResult;

			if (double.TryParse(sValue, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

	} // class DoubleFloat
} // namespace
