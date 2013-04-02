using System.IO;
using Aspose.Words;
using Nustache.Core;
using Scorto.Configuration;
using Scorto.Flow.Signal;
using log4net;

namespace EzBob.Signals.RenderAgreements
{
    public class RenderAgreementsHandler : SignalHandlerBase<RenderAgreementsMessage>
    {

        private readonly AgreementsStore _store = new AgreementsStore();

        private static readonly ILog log = LogManager.GetLogger("EzBob.Signals.RenderTemplates.RenderAgreementsHandler");

        public override void Execute()
        {
            log.InfoFormat("Rendering agreement");

            foreach (var item in Message.Items)
            {
                ProcessItem(item);
            }

            Common.RemoveMessage(Signal.Id, log);
        }

        private void ProcessItem(AgreementItem item)
        {
            if (string.IsNullOrEmpty(Message.RefNumber)) return;

            var html = Render.StringToString(item.Template, Message.CustomerData);
            var pdf = ConvertFormat(html, SaveFormat.Pdf);

            _store.SavePdf(pdf, Path.Combine(Config.PdfLoanAgreement, item.Filename));
            _store.SavePdf(pdf, Path.Combine(Config.PdfLoanAgreement2, item.Filename));
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
            return new RenderAgreementsHandler(){Config = this.Config};
        }
    }
}