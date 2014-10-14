namespace EzServiceCrontab.ArgumentTypes {
	internal class SingleInt : AType<int> {
		public SingleInt() : base("int") {}

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			int nResult;

			if (int.TryParse(sValue, out nResult))
				return nResult;

			throw GetError(sValue);
		} // CreateInstance

		#endregion method CreateInstance
	} // class SingleInt
} // namespace
