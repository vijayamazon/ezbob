namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal class LongIntish : AType<long?> {
		public LongIntish() : base("long") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			long nResult;

			if (long.TryParse(sValue, out nResult))
				return nResult;

			return Activator.CreateInstance(UnderlyingType);
		} // CreateInstance

		#endregion method CreateInstance
	} // class LongIntish
} // namespace
