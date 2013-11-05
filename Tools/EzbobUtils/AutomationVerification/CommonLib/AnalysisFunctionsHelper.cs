using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonLib
{
	class AnalysisFunctionsHelper
	{
		public double GetTurnoverForPeriod(List<AnalysisFunction> afs, TimePeriodEnum timePeriod)
		{
			double turnover = 0;
			double paypal = 0;
			double ebay = 0;

			foreach (var af in afs)
			{
				if (AnalysisFunctionIncome.IncomeFunctions.Contains(af.Function) && af.TimePeriod == timePeriod)
				{
					turnover += af.Value;
				}
			}
			return turnover;
		}
	}
}
