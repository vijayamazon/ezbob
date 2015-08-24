using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace EzBob.Web.Infrastructure.Html
{
    public class EzInputBuilder
    {

        public string Id { get; set; }
        public string Caption { get; set; }
        public string Value { get; set; }
        public string Type { get; set; }
        public string Cls { get; set; }
        public string Name { get; set; }
        public string UiEventControlID { get; set; }
        public string AutoCorrect { get; set; }
        public string AutoCapitalize { get; set; }
        public string FormFieldID { get; set; }
        public string InnerMessage { get; set; }
        public string LabelClass { get; set; }
        public int TabIndex { get; set; }
        public int MaxLength { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsRequired { get; set; }
        public bool StatusIcon { get; set; }
        public bool ToHide { get; set; }
        public EzButtonModel Button { get; set; }
        public EzInputBuilder() { }


        public EzInputBuilder(
            string id,
            string caption,
            string value = "",
            string type = "text",
            string cls = "",
              string name = "",
            string labelClass ="",
            string innerMessage = "",
            bool   isDisabled = false,
            bool   isRequired = false,
            bool   statusIcon = false,
            bool tohide = false,
            int   tabIndex = 0,
            int    maxLength = 0,
            string uiEventControlID = "",
            string autoCorrect = "",
            string autoCapitalize = "",
            string formFieldID = "",
             EzButtonModel button = null
            
            ) {
            Id = id;
            Caption = caption;
            Value = value;
            Type = type;
            Cls = cls;
            UiEventControlID = uiEventControlID;
            AutoCorrect = autoCorrect;
            AutoCapitalize = autoCapitalize;
            FormFieldID = formFieldID;
            TabIndex = tabIndex;
            MaxLength = maxLength;
            IsDisabled = isDisabled;
            IsRequired = isRequired;
            StatusIcon = statusIcon;
            InnerMessage = innerMessage;
            Button = button;
            ToHide = tohide;
            Name = name;
            LabelClass = labelClass;
        }




    }
}