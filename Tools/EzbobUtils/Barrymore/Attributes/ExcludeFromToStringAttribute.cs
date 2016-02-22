namespace Ezbob.Utils.Attributes {
	using System;

	[AttributeUsage(AttributeTargets.All)]
	public class ExcludeFromToStringAttribute : Attribute {

		public ExcludeFromToStringAttribute(bool print = true /*[CallerMemberName] string propertyName = null*/) {
			Print = print;
		}

		public bool Print { get; set; }
	}
}
