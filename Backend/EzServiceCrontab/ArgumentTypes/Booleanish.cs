namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal class Booleanish : AType<bool?> {
		public Booleanish() : base("bool") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			bool nResult;

			if (bool.TryParse(sValue, out nResult))
				return nResult;

			return Activator.CreateInstance(UnderlyingType);
		} // CreateInstance

		#endregion method CreateInstance
	} // class Booleanish
} // namespace
