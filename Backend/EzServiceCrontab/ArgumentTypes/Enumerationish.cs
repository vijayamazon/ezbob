namespace EzServiceCrontab.ArgumentTypes {
	using System;

	internal class Enumerationish : Enumeration {
		#region property CanBeNull

		public override bool CanBeNull {
			get { return true; }
		} // CanBeNull

		#endregion property CanBeNull

		#region property UnderlyingType

		public override Type UnderlyingType {
			get {
				return typeof (Nullable<>).MakeGenericType(new [] { base.UnderlyingType });
			} // get
		} // UnderlyingType

		#endregion property UnderlyingType

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			try {
				return Enum.Parse(UnderlyingType, sValue, true);
			}
			catch (Exception) {
				return Activator.CreateInstance(UnderlyingType);
			} // try
		} // CreateInstance

		#endregion method CreateInstance
	} // class Enumerationish
} // namespace
