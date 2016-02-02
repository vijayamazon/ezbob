using System;

namespace EzBobCommon.Web
{
    public interface IEzBobWebBrowser : IEzBobHttpClient, IDisposable {
        /// <summary>
        /// Sets the base address.
        /// </summary>
        /// <param name="url">The URL.</param>
        void SetBaseAddress(string url);
    }
}
