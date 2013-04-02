using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzBob.Web.Areas.Underwriter.Controllers.Reports
{
    public class CartImageProvider : IImageProvider
    {
        public string GetImagePath(object o)
        {
            if (o.ToString() == "Silver")
                return @"..\..\Content\img\cart_silver.png";
            if (o.ToString() == "Gold")
                return @"..\..\Content\img\cart_gold.png";
            if (o.ToString() == "Platinum")
                return @"..\..\Content\img\cart_platinum.png";
            if (o.ToString() == "Diamond")
                return @"..\..\Content\img\cart_diamond.png";
            if (o.ToString() == "Total")
                return null;
            throw new Exception("UnknownCartType");
        }
    }
}