namespace EzBob.Backend.Strategies.Misc {
	using System;
	using System.IO;
	using Aspose.Words;
	using EzBob.Backend.Models;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Nustache.Core;

	public class SaveAgreement : AStrategy {
		public SaveAgreement(
			int customerId,
			AgreementModel model,
			string refNumber,
			string name,
			TemplateModel template,
			string path1,
			string path2,
			AConnection oDb,
			ASafeLog oLog
		) : base(oDb, oLog) {
			_name = name;
			_template = template;
			_path1 = path1;
			_path2 = path2;
			_customerId = customerId;
			_model = model;
			_refNumber = refNumber;
		} // constructor

		public override string Name {
			get { return "Save Agreement"; }
		} // Name

		public override void Execute() {
			Log.Debug("Saving agreement for customer {0}...", _customerId);

			try {
				var html = Render.StringToString(_template.Template, _model);

				Log.Debug("Saving agreement for customer {0}: rendering done.", _customerId);

				var pdf = ConvertFormat(html, SaveFormat.Pdf);

				Save(pdf, _path1);

				Save(pdf, _path2);
			}
			catch (Exception exception) {
				Log.Error(exception, "Error saving agreement.");
			} // try

			Log.Debug("Saving agreement for customer {0} complete.", _customerId);
		} // Execute

		private void Save(byte[] pdf, string pathStr) {
			Log.Debug("Saving agreement for customer {0}: writing to {1}...", _customerId, pathStr);

			string path = Path.GetDirectoryName(pathStr);

			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			File.WriteAllBytes(pathStr, pdf);

			Log.Debug("Saving agreement for customer {0}: written to {1}.", _customerId, pathStr);
		} // Save

		private byte[] ConvertFormat(string stringForConvert, SaveFormat format, string typeInputString = "html") {
			Log.Debug("Saving agreement for customer {0}: converting format {1} -> {2}...", _customerId, typeInputString, format.ToString());

			var doc = new Document();
			var docBuilder = new DocumentBuilder(doc);

			if (typeInputString == "html")
				docBuilder.InsertHtml(stringForConvert);
			else
				docBuilder.Write(stringForConvert);

			byte[] oResult;

			using (var streamForDoc = new MemoryStream()) {
				doc.Save(streamForDoc, format);
				oResult = streamForDoc.ToArray();
			} // using

			Log.Debug("Saving agreement for customer {0}: converted format {1} -> {2}.", _customerId, typeInputString, format.ToString());
			return oResult;
		} // ConvertFormat

		private readonly string _name;
		private readonly TemplateModel _template;
		private readonly string _path1;
		private readonly string _path2;
		private int _customerId;
		private AgreementModel _model;
		private string _refNumber;
	} // class SaveAgreement
} // namespace
