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
			if (row.Table.Columns.Contains(index))
			{
				return safeParser.GetIntWithDefault(row[index], defaultValue);
			}

			return defaultValue;
		}

		public int Int(string index)
		{
			if (row.Table.Columns.Contains(index))
			{
				return safeParser.GetInt(row[index]);
			}

			return default(int);
		}

		public decimal DecimalWithDefault(string index, decimal defaultValue)
		{
			if (row.Table.Columns.Contains(index))
			{
				return safeParser.GetDecimalWithDefault(row[index], defaultValue);
			}

			return defaultValue;
		}

		public decimal Decimal(string index)
		{
			if (row.Table.Columns.Contains(index))
			{
				return safeParser.GetDecimal(row[index]);
			}

			return default(decimal);
		}

		public bool BoolWithDefault(string index, bool defaultValue)
		{
			if (row.Table.Columns.Contains(index))
			{
			return safeParser.GetBoolWithDefault(row[index], defaultValue);
			}

			return defaultValue;
		}

		public bool Bool(string index)
		{
			if (row.Table.Columns.Contains(index))
			{
			return safeParser.GetBool(row[index]);
			}

			return default(bool);
		}

		public DateTime DateTimeWithDefault(string index, DateTime defaultValue)
		{
			if (row.Table.Columns.Contains(index))
			{
			return safeParser.GetDateTimeWithDefault(row[index], defaultValue);
			}

			return defaultValue;
		}

		public DateTime DateTime(string index)
		{
			if (row.Table.Columns.Contains(index))
			{
				return safeParser.GetDateTime(row[index]);
			}

			return default(DateTime);
		}

		public string StringWithDefault(string index, string defaultValue)
		{
			if (row.Table.Columns.Contains(index))
			{
				return row[index].ToString();
			}

			return defaultValue;
		}

		public string String(string index)
		{
			if (row.Table.Columns.Contains(index))
			{
				return row[index].ToString();
			}

			return default(string);
		}
	}
}
