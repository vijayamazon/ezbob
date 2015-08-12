using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzBob.Web.Infrastructure.Html
{
    public class EzSelectModel
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public List<Tuple<int, string>> ListItems { get; set; }
        public string Cls { get; set; }
        public bool  IsDisabled { get; set; }
        public bool IsRequired { get; set; }
        public bool StatusIcon { get; set; }
        public int TabIndex { get; set; }
        public string UiEventControlID { get; set; }

        public EzSelectModel() { }

        public EzSelectModel(string id, 
            string caption, 
            string cls = "" , 
            List<Tuple<int, string>> listItems = null, 
            bool isDisabled = false,
            bool isRequired = false,
            bool statusIcon = false,
            int tabIndex = 0,
            string uiEventControlID = "")
        {
            Id = id;
            Caption = caption;
            Cls = cls;
            ListItems = listItems;
            IsDisabled = isDisabled;
            IsRequired = isRequired;
            StatusIcon = statusIcon; 
            TabIndex = tabIndex;
            UiEventControlID = uiEventControlID;

        }
    }
}