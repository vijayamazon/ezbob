using System.IO;
using Aspose.Words;
using EzBob.Signals.RenderAgreements;
using Nustache.Core;
using Scorto.Configuration;
using Scorto.Flow.Signal;
using log4net;

namespace EzBob.Signals.RenderConcentAgreement
{
    public class RenderConcentAgreementsHandler : SignalHandlerBase<RenderConcentAgreementsMessage>
    {

        private readonly AgreementsStore _store = new AgreementsStore();

        private static readonly ILog log = LogManager.GetLogger("EzBob.Signals.RenderTemplates.RenderConcentAgreementsHandler");

        public override void Execute()
        {
            log.InfoFormat("Rendering consent agreement");

            foreach (var item in Message.Items)
            {
                ProcessItem(item);
            }

            Common.RemoveMessage(Signal.Id, log);
        }

        private void ProcessItem(AgreementItem item)
        {
            var html = Render.StringToString(item.Template, Message.CustomerData);
            var pdf = ConvertFormat(html, SaveFormat.Pdf);

            _store.SavePdf(pdf, Path.Combine(Config.PdfConsentAgreement, item.Filename));
            _store.SavePdf(pdf, Path.Combine(Config.PdfConsentAgreement2, item.Filename));
        }

        public static byte[] ConvertFormat(string stringForConvert, SaveFormat format, string typeInputString = "html")
        {
            var doc = new Document();
            var docBuilder = new DocumentBuilder(doc);
            if (typeInputString == "html")
            {
                docBuilder.InsertHtml(stringForConvert);
            }
            else
            {
                docBuilder.Write(stringForConvert);
            }

            using (var streamForDoc = new MemoryStream())
            {
                doc.Save(streamForDoc, format);
                return streamForDoc.ToArray();
            }
        }

        public override void Init(string key)
        {
            Config = EnvironmentConfiguration.Configuration.GetConfiguration<RenderAgreementHandlerConfig>(key);
        }

        protected RenderAgreementHandlerConfig Config { get; set; }

        public override object Clone()
        {
            return new RenderConcentAgreementsHandler(){Config = this.Config};
        }
    }
}