namespace Ezbob.Utils
{
	using System;

	public static class MathUtils
	{
		public static decimal Round2DecimalDown(decimal value)
		{
			decimal tmp2 = Math.Round(value, 2);
			decimal tmp3 = Math.Truncate(value * 1000) / 1000;
			if (tmp2 - 0.005m == tmp3)
			{
				return tmp2 - 0.01m;
			}

			return tmp2;
		}

		public static decimal Round2DecimalDown(double value)
		{
			return Round2DecimalDown((decimal)value);
		}
	}
}
