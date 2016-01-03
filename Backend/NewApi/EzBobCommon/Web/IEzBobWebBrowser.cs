using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobCommon.Web
{
    public interface IEzBobWebBrowser : IEzBobHtmlClient, IDisposable {
        /// <summary>
        /// Sets the base address.
        /// </summary>
        /// <param name="url">The URL.</param>
        void SetBaseAddress(string url);
    }
}
