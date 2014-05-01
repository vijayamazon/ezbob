﻿namespace EzTvDashboard.Code
{
	public static class StringExtension
	{
		public static string ToK(this decimal num)
		{
			num = num/1000;
			return string.Format("{0}K",num.ToString("N0"));
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

	}
}