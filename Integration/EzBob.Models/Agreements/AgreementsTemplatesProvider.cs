namespace EzBob.Models.Agreements
{
	using System;
	using System.IO;

	public interface IAgreementsTemplatesProvider
    {
        string GetTemplateByName(string name);
    }

    public class AgreementsTemplatesProvider : IAgreementsTemplatesProvider
    {
        public string GetTemplateByName(string name)
        {
            return GetTemplate("\\Areas\\Customer\\Views\\Agreement\\", name);
        }

        public string GetTemplate(string path, string name)
        {
            return File.ReadAllText(string.Format("{0}{1}{2}.cshtml", AppDomain.CurrentDomain.BaseDirectory, path, name));
        }

    }
}