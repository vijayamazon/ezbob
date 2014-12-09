namespace EzServiceCrontab.ArgumentTypes {
	internal class Boolean : AType<bool> {
		public Boolean() : base("bool") {}

		public override object CreateInstance(string sValue) {
			bool nResult;

			if (bool.TryParse(sValue, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

	} // class Boolean
} // namespace
