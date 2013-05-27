using System.Web.Mvc;

namespace EzBob.Web.Infrastructure.Html
{
    public class InputTextBuilder : IHtmlRender
    {
        private string _label;
        private string _id;
        private string _name;
        private string _data_content;
        private string _css;

        public MvcHtmlString Render()
        {
            var groupTag = new TagBuilder("div");
            groupTag.AddCssClass("control-group");

            var label = new TagBuilder("label");
            label.AddCssClass("control-label");
            label.InnerHtml = _label;

            var controls = new TagBuilder("div");
            controls.AddCssClass("controls");

            var input = CreateInputTag();

            controls.InnerHtml = input.ToString(TagRenderMode.SelfClosing);
            groupTag.InnerHtml = label.ToString();
            groupTag.InnerHtml += controls.ToString();
            return MvcHtmlString.Create(groupTag.ToString());
        }

        private TagBuilder CreateInputTag()
        {
            var input = new TagBuilder("input");
            input.Attributes["type"] = "text";
            input.AddCssClass(_css);

            if (!string.IsNullOrEmpty(_id))
            {
                input.MergeAttribute("id", _id);
            }
            if (!string.IsNullOrEmpty(_name))
            {
                input.MergeAttribute("name", _name);
            }
            if (!string.IsNullOrEmpty(_data_content))
            {
                input.MergeAttribute("data-content", _data_content);
            }
            return input;
        }

        public InputTextBuilder Label(string label)
        {
            _label = label;
            return this;
        }

        public InputTextBuilder Id(string id)
        {
            _id = id;
            return this;
        }

        public InputTextBuilder Name(string name)
        {
            _name = name;
            return this;
        }

        public InputTextBuilder DataContent(string content)
        {
            _data_content = content;
            return this;
        }

        public InputTextBuilder Class(string classes)
        {
            _css = classes;
            return this;
        }
    }
}