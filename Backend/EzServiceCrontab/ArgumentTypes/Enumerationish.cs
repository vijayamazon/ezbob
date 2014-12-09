namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal class Enumerationish : Enumeration {

		public override bool CanBeNull {
			get { return true; }
		} // CanBeNull

		public override Type UnderlyingType {
			get {
				return typeof (Nullable<>).MakeGenericType(new [] { base.UnderlyingType });
			} // get
		} // UnderlyingType

		public override object CreateInstance(string sValue) {
			try {
				return Enum.Parse(UnderlyingType, sValue, true);
			}
			catch (Exception) {
				return Activator.CreateInstance(UnderlyingType);
			} // try
		} // CreateInstance

	} // class Enumerationish
} // namespace
