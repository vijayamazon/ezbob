using System;

namespace EzBob.Signals.RenderAgreements
{
    [Serializable]
    public class AgreementItem
    {
        public AgreementItem()
        {
        }

        public AgreementItem(string name, string template, string filename)
        {
            Name = name;
            Template = template;
            Filename = filename;
        }

        public string Template { get; set; }
        public string Filename { get; set; }
        public string Name { get; set; }
    }
}