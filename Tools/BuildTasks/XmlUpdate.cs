namespace BuildTasks
{
	using Microsoft.Build.Utilities;

	public class XmlUpdate : Task
	{
		public string XPath
		{
			get;
			set;
		}
		public string FilePath
		{
			get;
			set;
		}
		public string Value
		{
			get;
			set;
		}
		public override bool Execute()
		{
			bool result;
			if (!System.IO.File.Exists(this.FilePath))
			{
				base.Log.LogWarning("File not found: {0}, skip xml update.", new object[]
				{
					this.FilePath
				});
				result = true;
			}
			else
			{
				try
				{
					System.Xml.XmlDocument xmlDocument = new System.Xml.XmlDocument();
					xmlDocument.Load(this.FilePath);
					System.Xml.XmlNodeList xmlNodeList = xmlDocument.SelectNodes(this.XPath);
					if (xmlNodeList == null)
					{
						base.Log.LogWarning("XPath {0} selected 0 nodes, skip xml update.", new object[]
						{
							this.XPath
						});
						result = true;
						return result;
					}
					foreach (System.Xml.XmlNode xmlNode in xmlNodeList)
					{
						if (xmlNode is System.Xml.XmlElement)
						{
							xmlNode.InnerXml = this.Value;
						}
						if (xmlNode is System.Xml.XmlAttribute)
						{
							xmlNode.Value = this.Value;
						}
					}
					xmlDocument.Save(this.FilePath);
				}
				catch (System.Exception ex)
				{
					base.Log.LogWarning("Exception occured, skip xml update.", new object[0]);
					base.Log.LogWarning(ex.Message, new object[0]);
				}
				result = true;
			}
			return result;
		}
	}
}
