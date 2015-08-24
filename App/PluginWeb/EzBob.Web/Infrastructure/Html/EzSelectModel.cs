namespace EzBob.Web.Infrastructure.Html
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class EzSelectModel
    {
        public string Id { get; set; }
        public string Caption { get; set; }
        public List<EzSelectOptionGroup> ListItems { get; set; }
        public string Cls { get; set; }
        public string CustomAtts { get; set; }
        public bool IsDisabled { get; set; }
        public bool IsRequired { get; set; }
        public bool StatusIcon { get; set; }
        public int TabIndex { get; set; }
        public string UiEventControlID { get; set; }
        public string Name { get; set; }
        public Tuple<string, string> Placeholder { get; set; }
        public int Size { get; set; }
        public bool AutoFocus { get; set; }
        public bool HasEmpty { get; set; }

        public EzSelectModel() { }

        public EzSelectModel(string id,
            string caption,
            List<EzSelectOptionGroup> listItems,
            string cls = "",
            bool isDisabled = false,
            bool isRequired = false,
            bool statusIcon = false,
            int tabIndex = 0,
            string uiEventControlID = "",
            string customAtts = "",
            string name = "",
            Tuple<string, string> placeholder = null,
            bool autoFocus = false,
            int size = 1,
            bool hasEmpty = true)
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
            CustomAtts = customAtts;
            Name = name;
            Placeholder = placeholder;
            AutoFocus = autoFocus;
            Size = size;
            HasEmpty = hasEmpty;
        }



        public EzSelectModel(string id,
            string caption,
            List<Tuple<string, string>> listItems,
            string cls = "",
            bool isDisabled = false,
            bool isRequired = false,
            bool statusIcon = false,
            int tabIndex = 0,
            string customAtts = "",
            string uiEventControlID = "",
            string name = "",
            Tuple<string, string> placeholder = null,
            bool autoFocus = false,
            int size = 1,
            bool hasEmpty = true)
            : this(id,
            caption,
            new List<EzSelectOptionGroup> { 
				new EzSelectOptionGroup { 
					Options = listItems.Select(x => new EzSelectOption(x.Item1, x.Item2)).ToList()
				}
			},
            cls,
            isDisabled,
            isRequired,
            statusIcon,
            tabIndex,
            uiEventControlID,
            customAtts,
            name,
            placeholder,
            autoFocus,
            size,
            hasEmpty) { }
    }

    public class EzSelectOption
    {
        public string Value { get; set; }
        public string Text { get; set; }
        public string Cls { get; set; }

        public EzSelectOption(string value, string text, string cls = "")
        {
            this.Value = value;
            this.Text = text;
            this.Cls = cls;
        }
    }

    public class EzSelectOptionGroup
    {
        public List<EzSelectOption> Options { get; set; }
        public string GroupTitle { get; set; }


    }
}