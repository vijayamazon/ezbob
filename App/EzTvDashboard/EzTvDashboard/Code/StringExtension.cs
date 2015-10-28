namespace EzTvDashboard.Code
{
	using System;
	using System.Collections.Generic;

	public static class StringExtension
	{
		public static string ToK0(this decimal num)
		{
			num = num/1000;
			return string.Format("{0}K",num.ToString("N0"));
		}

		public static string ToK1(this decimal num)
		{
			num = num / 1000;
			return string.Format("{0}K", num.ToString("N1"));
		}

		public static string ToK2(this decimal num)
		{
			num = num / 1000;
			return string.Format("{0}K", num.ToString("N2"));
		}

		public static string ToM(this decimal num)
		{
			num = num / 1000000;
			return string.Format("{0}M", num.ToString("N2"));
		}

		public static string ToPercent(this decimal num)
		{
			num = num * 100;
			return string.Format("{0}%", num.ToString("N2"));
		}

		public static string ToInt(this decimal num)
		{
			return num.ToString("N0");
		}

		public static int ToRoundIntPercent(this decimal num, decimal value) {
			return value == 0 ? 0 : (int)Math.Round(num / value * 100);
		}

		public static decimal GetSafe(this Dictionary<string, decimal> dict, string key)
		{
			if (dict == null)
			{
				return 0;
			}

			if (dict.ContainsKey(key))
			{
				return dict[key];
			}
			return 0;
		}

	}
}