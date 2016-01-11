namespace EzBob.Web.Code {
	using System;
	using System.IO;
	using Aspose.Words;
	using EzBob.Backend.Models;
	using Nustache.Core;

	public class AgreementRenderer {
		public static byte[] ConvertToPdf(string htmlForConvert) {
			return ConvertFormat(htmlForConvert, SaveFormat.Pdf);
		}

		public static byte[] ConvertFormat(string htmlForConvert, SaveFormat format) {
			var doc = new Document();
			var docBuilder = new DocumentBuilder(doc);
			docBuilder.InsertHtml(htmlForConvert);

			using (var streamForDoc = new MemoryStream()) {
				doc.Save(streamForDoc, format);
				return streamForDoc.ToArray();
			}
		}

		public static byte[] ConvertToPdf(
			byte[] file,
			LoadFormat loadFormat = LoadFormat.Auto,
			SaveFormat saveFormat = SaveFormat.Pdf
		) {
			try {
				var stream = new MemoryStream(file);
				var doc = new Document(stream, null, loadFormat, null);

				using (var streamForDoc = new MemoryStream()) {
					doc.Save(streamForDoc, saveFormat);
					return streamForDoc.ToArray();
				}
			} catch (Exception) {
				return null;
			}
		}

		public byte[] RenderAgreementToPdf(string view, AgreementModel model) {
			return ConvertToPdf(RenderAgreement(view, model));
		}

		public string RenderAgreement(string view, AgreementModel model) {
			var html = Render.StringToString(view, model);
			return html;
		}

		public string AggrementToBase64String(string view, AgreementModel model) {
			byte[] pdfData = ConvertToPdf(RenderAgreement(view, model));
			return Convert.ToBase64String(pdfData);
		}
	}
}
