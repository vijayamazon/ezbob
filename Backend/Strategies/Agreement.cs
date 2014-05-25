namespace EzBob.Backend.Strategies
{
	using System;
	using System.IO;
	using Aspose.Words;
	using EzBob.Backend.Models;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Nustache.Core;

	public class SaveAgreement : AStrategy
	{
		private readonly string _name;
		private readonly TemplateModel _template;
		private readonly string _path1;
		private readonly string _path2;
		private int _customerId;
		private AgreementModel _model;
		private string _refNumber;

		public SaveAgreement(int customerId, AgreementModel model, string refNumber, string name, TemplateModel template, string path1, string path2, AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			_name = name;
			_template = template;
			_path1 = path1;
			_path2 = path2;
			_customerId = customerId;
			_model = model;
			_refNumber = refNumber;
		}

		public override string Name
		{
			get { return "Save Agreement"; }
		}

		public override void Execute()
		{
			try
			{
				var html = Render.StringToString(_template.Template, _model);
				var pdf = ConvertFormat(html, SaveFormat.Pdf);
				Save(pdf, _path1);
				Save(pdf, _path2);
			}
			catch (Exception exception)
			{
				Log.Error("Error saving agreement", exception.Message);
			}
		}

		private void Save(byte[] pdf, string pathStr)
		{
			var path = Path.GetDirectoryName(pathStr);
			if (!Directory.Exists(path)) Directory.CreateDirectory(path);
			File.WriteAllBytes(pathStr, pdf);
		}

		private byte[] ConvertFormat(string stringForConvert, SaveFormat format, string typeInputString = "html")
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
	}
}
