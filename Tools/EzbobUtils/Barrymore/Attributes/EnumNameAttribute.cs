namespace Ezbob.Utils.Attributes {
	using System;

	[AttributeUsage(AttributeTargets.Property)]
	public class EnumNameAttribute : Attribute {

		public string Name { get; set; }
		private readonly Type enumType;

		public EnumNameAttribute(Type enumType) {
			this.enumType = enumType;
		}

		public string GetName(int propertyValue = 0) {
			try {
				Name = Enum.GetName(this.enumType, Convert.ToInt32(propertyValue));
				return Name;
				// ReSharper disable once CatchAllClause
			} catch (FormatException e) {
				// enum value not valid
				Console.WriteLine(e);
			} 
			return Name;
		}


	}
}
