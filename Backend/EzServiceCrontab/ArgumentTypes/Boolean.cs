namespace EzServiceCrontab.ArgumentTypes {
	internal class Boolean : AType<bool> {
		public Boolean() : base("bool") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			bool nResult;

			if (bool.TryParse(sValue, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

		#endregion method CreateInstance
	} // class Boolean
} // namespace
