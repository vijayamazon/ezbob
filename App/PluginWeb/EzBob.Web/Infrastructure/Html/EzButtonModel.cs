using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzBob.Web.Infrastructure.Html
{
    public class EzButtonModel
    {
          public string Id { get; set; }
        public string Caption { get; set; }
        public string Cls { get; set; }
        public string UiEventControlID { get; set; }

        public EzButtonModel() { }

        public EzButtonModel(string id, string caption, string cls, string uiEventControlID) {
            Id = id;
            Caption = caption;
            Cls = cls;
            UiEventControlID = uiEventControlID;
        }
    }
}