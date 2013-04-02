using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EzBob.Web.Areas.Underwriter.Controllers.Reports
{
    public interface IImageProvider
    {
        string GetImagePath(object o);
    }
}
