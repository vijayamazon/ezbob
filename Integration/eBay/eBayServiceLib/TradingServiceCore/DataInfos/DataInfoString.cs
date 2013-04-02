namespace EzBob.eBayServiceLib.TradingServiceCore.DataInfos
{
	public abstract class DataInfoString : DataInfo<string>
	{
		protected DataInfoString( string value ) :
			base( value )
		{
		}

		public override bool HasData
		{
			get { return !string.IsNullOrWhiteSpace( Value ); }
		}

	}
}