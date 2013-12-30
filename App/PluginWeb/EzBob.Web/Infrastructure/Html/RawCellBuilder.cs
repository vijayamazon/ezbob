using System.Text;
using System.Web.Mvc;

namespace EzBob.Web.Infrastructure.Html
{
    public class RawCellBuilder : IHtmlRender
    {
        private string _title;
        private string _value;
        private string _buttonName;
        private string _tooltip;
	    private StringBuilder _rawTail;

		public RawCellBuilder() {
			_rawTail = new StringBuilder();
		}

		public RawCellBuilder AddRawTail(string sHtml) {
			if (!string.IsNullOrWhiteSpace(sHtml))
				_rawTail.Append(sHtml);
			return this;
		}

        public MvcHtmlString Render()
        {
            var row = new TagBuilder("tr");
            var td1 = CreateLeftTd();
            var td2 = CreateRightTd();
            row.InnerHtml += td1.ToString();
	        row.InnerHtml += td2.ToString();
            return MvcHtmlString.Create(row.ToString());
        }

        public RawCellBuilder Title(string title)
        {
            _title = title;
            return this;
        }

        public RawCellBuilder Value(string value)
        {
            _value = value;
            return this;
        }

        public RawCellBuilder Button(string name)
        {
            _buttonName = name;
            return this;
        }

        public RawCellBuilder Tooltip(string text)
        {
            _tooltip = text;
            return this;
        }

        private TagBuilder CreateLeftTd()
        {
            var td = new TagBuilder("td");
            td.InnerHtml = _title;
            return td;
        }

        private TagBuilder CreateRightTd()
        {
            var td = new TagBuilder("td");
            td.InnerHtml += _value;
            AddTooltip(td);
            AddButton(td);
            td.InnerHtml += _rawTail.ToString();
            return td;
        }

        private void AddTooltip(TagBuilder td)
        {
            if (string.IsNullOrEmpty(_tooltip)) return;
            var tooltip = string.Format(" <i data-title='{0}' class='tltp-left icon-question-sign' data-original-title=''></i> ",
                              _tooltip);
            td.InnerHtml += tooltip;
        }

        private void AddButton(TagBuilder td)
        {
            if (string.IsNullOrEmpty(_buttonName)) return;
            var button = new TagBuilder("button");
            button.MergeAttribute("name", _buttonName);
            button.AddCssClass("btn btn-mini btn-primary");
			button.InnerHtml = @"<i class='fa fa-pencil'/>";
            td.InnerHtml += button.ToString();
        }
    }
}