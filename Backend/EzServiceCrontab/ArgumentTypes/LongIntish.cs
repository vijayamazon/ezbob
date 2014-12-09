namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal class LongIntish : AType<long?> {
		public LongIntish() : base("long") {}

		public override object CreateInstance(string sValue) {
			long nResult;

			if (long.TryParse(sValue, out nResult))
				return nResult;

			return Activator.CreateInstance(UnderlyingType);
		} // CreateInstance

	} // class LongIntish
} // namespace
