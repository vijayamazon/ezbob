using System.IO;
using Aspose.Words;
using EzBob.Web.Areas.Customer.Models;
using Nustache.Core;

namespace EzBob.Web.Code.Agreements
{
	using System;

	public class AgreementRenderer
    {

        public byte[] RenderAgreementToPdf(string view, AgreementModel model)
        {
            return ConvertToPdf(RenderAgreement(view, model));
        }

        public string RenderAgreement(string view, AgreementModel model)
        {
            var html = Render.StringToString(view, model);
            return html;
        }

		public string AggrementToBase64String(string view, AgreementModel model)
		{
			byte[] pdfData = ConvertToPdf(RenderAgreement(view, model));
			return Convert.ToBase64String(pdfData);
		}

        public static byte[] ConvertToPdf(string htmlForConvert)
        {
            return ConvertFormat(htmlForConvert, SaveFormat.Pdf);
        }

        public static byte[] ConvertFormat(string htmlForConvert, SaveFormat format)
        {
            var doc = new Document();
            var docBuilder = new DocumentBuilder(doc);
            docBuilder.InsertHtml(htmlForConvert);
            
            using (var streamForDoc = new MemoryStream())
            {
                doc.Save(streamForDoc, format);
                return streamForDoc.ToArray();
            }            
        }
    }
}