using System;
using System.Text;
using Html.Attributes;

namespace Html.Tags {
	public class Html : ATag { public override string Tag { get { return "html"; } } }
	public class Head : ATag { public override string Tag { get { return "head"; } } }
	public class Body : ATag { public override string Tag { get { return "body"; } } }

	public class H1 : ATag { public override string Tag { get { return "h1"; } } }
	public class H2 : ATag { public override string Tag { get { return "h2"; } } }
	public class H3 : ATag { public override string Tag { get { return "h3"; } } }
	public class H4 : ATag { public override string Tag { get { return "h4"; } } }
	public class H5 : ATag { public override string Tag { get { return "h5"; } } }
	public class H6 : ATag { public override string Tag { get { return "h6"; } } }

	public class P : ATag { public override string Tag { get { return "p"; } } }
	public class Div : ATag { public override string Tag { get { return "div"; } } }
	public class Span : ATag { public override string Tag { get { return "span"; } } }

	public class Table : ATag { public override string Tag { get { return "table"; } } }
	public class Thead : ATag { public override string Tag { get { return "thead"; } } }
	public class Tbody : ATag { public override string Tag { get { return "tbody"; } } }
	public class Tr : ATag { public override string Tag { get { return "tr"; } } }
	public class Th : ATag { public override string Tag { get { return "th"; } } }
	public class Td : ATag { public override string Tag { get { return "td"; } } }

	public class A : ATag {
		public A() { Href = new Href(); Title = new Title(); Alt = new Alt(); Target = new Target(); }
		public override string Tag { get { return "a"; } }
		public virtual Alt Alt { get; private set; }
		public virtual Href Href { get; private set; }
		public virtual Target Target { get; private set; }
		public virtual Title Title { get; private set; }
	} // class A

	public class Img : ATag {
		public Img() { Src = new Src(); }
		public override bool MustClose { get { return false; } }
		public override string Tag { get { return "img"; } }
		public virtual Src Src { get; private set; }
	} // class Img

	public class Text : ATag {
		public Text(string sContent = null) {
			Content = new StringBuilder();

			if (sContent != null)
				Append(sContent);
		} // constructor

		public override ATag Append(ATag oTag) {
			throw new NotImplementedException();
		} // Append
		
		public virtual ATag Append(string sContent) {
			Content.Append(sContent ?? "--null--");
			return this;
		} // Append

		public override string ToString() {
			return Content.ToString();
		} // ToString

		public override string Tag { get { return ""; } }

		private StringBuilder Content { get; set; }
	} // Text

	public class Style : Text {
		public Style(string sContent = null) : base(sContent) {}

		public override string Tag { get { return "style"; } }

		public override string ToString() {
			return "<" + Tag + ">" + base.ToString() + "</" + Tag + ">";
		} // ToString
	} // Style
} // namespace Html.Tags
