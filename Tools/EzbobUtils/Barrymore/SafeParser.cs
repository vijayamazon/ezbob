namespace Ezbob.Utils
{
	using System;

	public class SafeParser
	{
		public int GetIntWithDefault(object obj, int defaultValue)
		{
			if (obj == null)
			{
				return defaultValue;
			}

			int result;
			if (!int.TryParse(obj.ToString(), out result))
			{
				return defaultValue;
			}

			return result;
		}

		public int GetInt(object obj)
		{
			return GetIntWithDefault(obj, default(int));
		}

		public decimal GetDecimalWithDefault(object obj, decimal defaultValue)
		{
			if (obj == null)
			{
				return defaultValue;
			}

			decimal result;
			if (!decimal.TryParse(obj.ToString(), out result))
			{
				return defaultValue;
			}

			return result;
		}

		public decimal GetDecimal(object obj)
		{
			return GetDecimalWithDefault(obj, default(decimal));
		}

		public bool GetBoolWithDefault(object obj, bool defaultValue)
		{
			if (obj == null)
			{
				return defaultValue;
			}

			bool result;
			if (!bool.TryParse(obj.ToString(), out result))
			{
				return defaultValue;
			}

			return result;
		}

		public bool GetBool(object obj)
		{
			return GetBoolWithDefault(obj, default(bool));
		}

		public DateTime GetDateTimeWithDefault(object obj, DateTime defaultValue)
		{
			if (obj == null)
			{
				return defaultValue;
			}

			DateTime result;
			if (!DateTime.TryParse(obj.ToString(), out result))
			{
				return defaultValue;
			}

			return result;
		}

		public DateTime GetDateTime(object obj)
		{
			return GetDateTimeWithDefault(obj, default(DateTime));
		}
	}
}
