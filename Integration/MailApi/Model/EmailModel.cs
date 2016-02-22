using System.Collections.Generic;

// ReSharper disable InconsistentNaming

namespace MailApi.Model {
	using System.Linq;

	public class merge_var {
		public string name;
		public string content;

		public override string ToString() {
			return string.Format(
				"{{ variable name: '{0}', content: '{1}' }}", 
				name ?? "-- null --",
				content ?? "-- null --"
			);
		} // ToString
	} // class merge_var

	public class template_content {
		public string name;
		public string content;

		public override string ToString() {
			return string.Format("{{ content name: '{0}', content: '{1}' }}", name ?? "-- null --", content ?? "-- null --");
		} // ToString
	} // class template_content

	public class attachment {
		public string type;
		public string name;
		public string content;

		public override string ToString() {
			return string.Format("{{ attachment name: '{0}', type: '{1}' }}", name ?? "-- null --", type ?? "-- null --");
		} // ToString
	} // class attachment

	public class image {
		public string type;
		public string name;
		public string content;

		public override string ToString() {
			return string.Format("{{ attachment name: '{0}', type: '{1}' }}", name ?? "-- null --", type ?? "-- null --");
		} // ToString
	} // class image

	public class EmailModel {
		public EmailModel() {
			template_content = new[] { new template_content { name = "" } };
		} // constructor

		public string key { get; set; }
		public string template_name { get; set; }
		public IEnumerable<template_content> template_content { get; set; }
		public EmailMessageModel message { get; set; }

		public void AddGlobalVariable(string name, string content) {
			if (message.global_merge_vars == null)
				message.global_merge_vars = new List<merge_var>();

			var mv = new merge_var {
				name = name,
				content = content,
			};

			message.global_merge_vars.Add(mv);
		} // AddGlobalVariable

		public void AddAttachment(string mimeType, string fileName, string content) {
			if (message.attachments == null)
				message.attachments = new List<attachment>();

			message.attachments.Add(new attachment {
				type = mimeType,
				name = fileName,
				content = content
			});
		} // AddAttachment

		public override string ToString() {
			List<string> oTemplateContent = template_content == null
				? new List<string>()
				: template_content.Select(tc => tc.ToString()).ToList();

			string sTemplateContent = oTemplateContent.Count < 1
				? "empty template content"
				: string.Format("[{0}] {{ {1} }}", oTemplateContent.Count, string.Join(", ", oTemplateContent));

			return string.Format(
				"Key: {0}, Template name: '{1}', Template content: {2}, Message: {3}",
				key,
				template_name,
				sTemplateContent,
				message
			);
		} // ToString
	} // class EmailModel
} // namespace

// ReSharper restore InconsistentNaming