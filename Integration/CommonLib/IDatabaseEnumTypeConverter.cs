namespace EzBob.CommonLib
{
	public interface IDatabaseEnumTypeConverter<in T>
	{
		ConvertedTypeInfo Convert( T type );
	}
}