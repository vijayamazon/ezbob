using System;
using System.IO;
using EzBob.Web.Code.Agreements;

namespace ezmanage
{
    public class EzAgreementsTemplateProvider : IAgreementsTemplatesProvider
    {
        public string GetTemplateByName(string name)
        {
            return File.ReadAllText(string.Format("{0}\\Agreement\\{1}.cshtml", AppDomain.CurrentDomain.BaseDirectory, name));
        }
    }
}