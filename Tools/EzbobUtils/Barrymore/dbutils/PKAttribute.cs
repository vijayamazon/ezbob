namespace Ezbob.Utils.dbutils {
	using System;

	[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
	public class PKAttribute : Attribute {
		public PKAttribute(bool withItendtity = false) {
			WithIdentity = withItendtity;
		} // constructor

		public bool WithIdentity { get; private set; }
	} // class PKAttribute
} // namespace
