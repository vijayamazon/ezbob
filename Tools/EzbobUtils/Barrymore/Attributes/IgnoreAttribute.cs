namespace Ezbob.Utils.Attributes {
	using System;

	[AttributeUsage(AttributeTargets.Property)]
	public class Ignore : Attribute {
		private string mPropertyName;
		public Ignore(string propertyName) {
			this.mPropertyName = propertyName;
		}
	}
}
