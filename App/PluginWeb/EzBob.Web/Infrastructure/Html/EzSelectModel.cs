﻿using System.Collections.Generic;

namespace EzBob.Web.Infrastructure.Html
{
	using System;
	using System.Linq;

	public class EzSelectModel
    {
        public string Id { get; set; }
        public string Caption { get; set; }
		public List<EzSelectOptionGroup> ListItems { get; set; }
        public string Cls { get; set; }
        public bool  IsDisabled { get; set; }
        public bool IsRequired { get; set; }
        public bool StatusIcon { get; set; }
        public int TabIndex { get; set; }
        public string UiEventControlID { get; set; }

        public EzSelectModel() { }

        public EzSelectModel(string id, 
            string caption,
			List<EzSelectOptionGroup> listItems, 
			string cls = "" ,
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

		public EzSelectModel(string id,
			string caption,
			List<Tuple<string, string>> listItems,
			string cls = "",
			bool isDisabled = false,
			bool isRequired = false,
			bool statusIcon = false,
			int tabIndex = 0,
			string uiEventControlID = "") : this (id,
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
			uiEventControlID) { }
    }

	public class EzSelectOption {
		public string Value { get; set; }
		public string Text { get; set; }
		public string Cls { get; set; }

		public EzSelectOption(string value, string text, string cls = "") {
			this.Value = value;
			this.Text = text;
			this.Cls = cls;
		}
	}

	public class EzSelectOptionGroup {
		public List<EzSelectOption> Options { get; set; }
		public string GroupTitle { get; set; }
	}
}