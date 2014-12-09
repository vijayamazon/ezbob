using System;
using System.Collections.Generic;

namespace Integration.ChannelGrabberConfig {

	public class LinkForm : ICloneable {

		public LinkForm() {
			Fields = new List<FieldInfo>();
			Notes = new List<string>();
			HasUploadFiles = false;
			UploadFilesHandler = "";
			OnBeforeLink = new List<string>();
		} // constructor

		public List<FieldInfo> Fields { get; set; }
		public List<string> Notes { get; set; }
		public bool HasUploadFiles { get; set; }
		public string UploadFilesHandler { get; set; }
		public List<string> OnBeforeLink { get; set; }

		public void Validate() {
			Fields = Fields ?? new List<FieldInfo>();
			Notes = Notes ?? new List<string>();
			OnBeforeLink = OnBeforeLink ?? new List<string>();

			if (Fields.Count < 1)
				throw new ConfigException("Fields not specified.");

			foreach (FieldInfo fi in Fields)
				fi.Validate();

		} // Validate

		public override string ToString() {
			return string.Format(
				"Has upload files: {0}{2}\n{1}",
				HasUploadFiles ? "yes" : "no",
				Fields,
				HasUploadFiles ? " to " + UploadFilesHandler : ""
			);
		} // ToString

		public object Clone() {
			var oRes = new LinkForm();

			Fields.ForEach( fi => oRes.Fields.Add((FieldInfo)fi.Clone()) );

			Notes.ForEach( s => oRes.Notes.Add((string)s.Clone()) );

			oRes.HasUploadFiles = HasUploadFiles;

			oRes.UploadFilesHandler = UploadFilesHandler;

			OnBeforeLink.ForEach( s => oRes.OnBeforeLink.Add((string)s.Clone()) );

			return oRes;
		} // Clone

	} // class LinkForm

} // namespace Integration.ChannelGrabberConfig
