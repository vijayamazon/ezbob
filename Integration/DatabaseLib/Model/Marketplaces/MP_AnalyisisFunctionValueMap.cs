using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database 
{
    public class MP_AnalyisisFunctionValueMap : ClassMap<MP_AnalyisisFunctionValue> 
	{        
        public MP_AnalyisisFunctionValueMap() 
		{
			Table( "MP_AnalyisisFunctionValues" );
			Id( x => x.Id );
			References(x => x.CustomerMarketPlace, "CustomerMarketPlaceId");
			References( x => x.AnalyisisFunction, "AnalyisisFunctionId" );
			References( x => x.AnalysisFunctionTimePeriod, "AnalysisFunctionTimePeriodId" );
			Map(x => x.Updated).Not.Nullable();
			Map(x => x.ValueString);
			Map(x => x.ValueInt);
			Map(x => x.ValueFloat);
			Map(x => x.ValueDate);
			Map(x => x.ValueXml);
			Map( x => x.ValueBoolean );
			Map( x => x.Value );
			Map( x => x.CountMonths );			
			References( x => x.HistoryRecord, "CustomerMarketPlaceUpdatingHistoryRecordId" );
            Cache.ReadWrite().Region("LongTerm");
		}
    }

	/*public class SqlVariant : IUserType
	{

		private SqlType[] variantSqlType = new[] { new SqlType( DbType.Object ) };

		public SqlType[] SqlTypes
		{
			get
			{
				return variantSqlType;
			}
		}

		public System.Type ReturnedType
		{
			get { return typeof( object ); }
		}

		public new bool Equals( object x, object y )
		{
			return object.Equals( x, y );
		}

		public int GetHashCode( object x )
		{
			return x.GetHashCode();
		}

		public object NullSafeGet( IDataReader rs, string[] names, object owner )
		{
			int ordinal = rs.GetOrdinal( names[0] );
			if ( rs.IsDBNull( ordinal ) )
			{
				return null;
			}
			else
			{
				return rs[ordinal];
			}
		}

		public void NullSafeSet( IDbCommand cmd, object value, int index )
		{

			object valueToSet = value ?? DBNull.Value;
			( (IDbDataParameter)cmd.Parameters[index] ).Value = valueToSet;
		}

		public object DeepCopy( object value )
		{
			return value;
		}

		public bool IsMutable
		{
			get { return false; }
		}

		public object Replace( object original, object target, object owner )
		{
			return original;
		}

		public object Assemble( object cached, object owner )
		{
			return cached;
		}

		public object Disassemble( object value )
		{
			return value;
		}

	}*/
}
