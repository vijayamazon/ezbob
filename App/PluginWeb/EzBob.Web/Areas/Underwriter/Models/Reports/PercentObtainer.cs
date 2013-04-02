using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzBob.Web.Areas.Underwriter.Models.Reports
{
    public class PercentObtainer
    {
        protected decimal GetPercent(int x, int y)
        {
            return y != 0 ? x / (decimal)y : 0.00m;
        }

        protected decimal GetPercent(double x, double y)
        {
            return Math.Abs(y - 0.00) < 0.00001 ? 0.00m : (decimal) (x / y);
        }

        protected decimal GetPercent(decimal x, decimal y)
        {
            return  y == 0.00m ? 0.00m : x / y;
        }
    }
}