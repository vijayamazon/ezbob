namespace BuildTasks
{
	using System;
	using System.IO;
	using System.Text;
	using System.Xml;
	using Commons.Collections;
	using Microsoft.Build.Framework;
	using Microsoft.Build.Utilities;
	using NVelocity;
	using NVelocity.App;
	using NVelocity.Runtime;
	using NVelocity.Runtime.Resource.Loader;

	public class TemplateTask : Task
	{
		public string DataXml { get; set; }
		public string OutXpath { get; set; }

		[Required]
		public string TemplatePath { get; set; }

		[Required]
		public string OutPath { get; set; }

		public override bool Execute()
		{
			Log.LogMessage(MessageImportance.High, "Processing template: {0}...", TemplatePath);

			if (!File.Exists(TemplatePath))
			{
				Log.LogError("Template path is invalid: {0}", TemplatePath);
				return false;
			}

			var extendedProperties = new ExtendedProperties();
			extendedProperties.AddProperty("resource.loader", "file");
			extendedProperties.AddProperty("file.resource.loader.class", typeof(FileResourceLoader).FullName);
			extendedProperties.AddProperty("file.resource.loader.path", Path.GetDirectoryName(TemplatePath));

			var velocityEngine = new VelocityEngine(extendedProperties);
			RuntimeSingleton.Configuration["file.resource.loader.path"] = Path.GetDirectoryName(TemplatePath);
			var velocityContext = new VelocityContext();

			var xmlDocument = new XmlDocument();
			xmlDocument.LoadXml(DataXml);
			foreach (XmlElement xmlElement in xmlDocument.FirstChild.ChildNodes)
			{
				velocityContext.Put(xmlElement.Name, xmlElement.InnerText);
			}

			try
			{
				using (var stringWriter = new StringWriter())
				{
					velocityEngine.MergeTemplate(Path.GetFileName(TemplatePath), Encoding.UTF8.WebName, velocityContext, stringWriter);
					Log.LogMessage(MessageImportance.High, "Writting file: {0}...", OutPath);
					if (string.IsNullOrEmpty(OutXpath))
					{
						File.WriteAllText(OutPath, stringWriter.ToString());
					}
					else
					{
						xmlDocument = new XmlDocument();
						xmlDocument.LoadXml(stringWriter.ToString());
						var envionmentXmlElement = xmlDocument.SelectSingleNode("/environment") as XmlElement;
						if (envionmentXmlElement == null)
						{
							Log.LogError("Invalid xml in source template {0}", TemplatePath);
							return false;
						}
						if (!File.Exists(OutPath))
						{
							Log.LogError("Xml file {0} not found, to apply xpath: {1}", OutPath, OutXpath);
							return false;
						}
						var newXmlDocument = new XmlDocument();
						newXmlDocument.Load(OutPath);
						var xmlNodeList = newXmlDocument.SelectNodes(OutXpath);
						if (xmlNodeList != null)
						{
							foreach (XmlElement xmlElement in xmlNodeList)
							{
								xmlElement.InnerXml = envionmentXmlElement.InnerXml;
							}
						}
						newXmlDocument.Save(OutPath);
					}
				}
			}
			catch (Exception exception)
			{
				Log.LogError("Failed to create file from template: {0}", TemplatePath);
				Log.LogErrorFromException(exception);
				return false;
			}
			return true;
		}
	}
}
