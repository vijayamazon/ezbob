using System;
using System.IO;
using log4net;

namespace EzBob.Signals.RenderAgreements
{
    public class AgreementsStore
    {
        private static readonly ILog log = LogManager.GetLogger("EzBob.Signals.RenderTemplates.AgreementsStore");

        public void SavePdf(byte[] body, string filename)
        {
            try
            {
                var path = Path.GetDirectoryName(filename);
                if (!Directory.Exists(path)) Directory.CreateDirectory(path);
                File.WriteAllBytes(filename, body);
            }
            catch (Exception exception)
            {
                log.ErrorFormat(exception.Message);
            }
            
        }
    }
}