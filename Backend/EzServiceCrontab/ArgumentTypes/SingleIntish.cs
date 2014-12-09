namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal class SingleIntish : AType<int?> {
		public SingleIntish() : base("int") {}

		public override object CreateInstance(string sValue) {
			int nResult;

			if (int.TryParse(sValue, out nResult))
				return nResult;

			return Activator.CreateInstance(UnderlyingType);
		} // CreateInstance

	} // class SingleIntish
} // namespace
