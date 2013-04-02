namespace EzBob.CommonLib
{
	public class ConvertedTypeInfo
	{
		public ConvertedTypeInfo()
		{
		}

		public ConvertedTypeInfo( string name, string displayName, string description )
		{
			Name = name;
			DisplayName = displayName;
			Description = description;
		}

		public string Name { get; set; }
		public string DisplayName { get; set; }
		public string Description { get; set; }
	}
}