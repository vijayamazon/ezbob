namespace Ezbob.Database 
{
	using System;
	using System.Data;
	using Utils;

	public class SafeReader
	{
		private readonly DataRow row;
		private readonly SafeParser safeParser = new SafeParser();

		public SafeReader(DataRow row)
		{
			this.row = row;
		}

		public int IntWithDefault(string index, int defaultValue)
		{
			return safeParser.GetIntWithDefault(row[index], defaultValue);
		}

		public int Int(string index)
		{
			return safeParser.GetInt(row[index]);
		}

		public decimal DecimalWithDefault(string index, decimal defaultValue)
		{
			return safeParser.GetDecimalWithDefault(row[index], defaultValue);
		}

		public decimal Decimal(string index)
		{
			return safeParser.GetDecimal(row[index]);
		}

		public bool BoolWithDefault(string index, bool defaultValue)
		{
			return safeParser.GetBoolWithDefault(row[index], defaultValue);
		}

		public bool Bool(string index)
		{
			return safeParser.GetBool(row[index]);
		}

		public DateTime DateTimeWithDefault(string index, DateTime defaultValue)
		{
			return safeParser.GetDateTimeWithDefault(row[index], defaultValue);
		}

		public DateTime DateTime(string index)
		{
			return safeParser.GetDateTime(row[index]);
		}
	}
}
