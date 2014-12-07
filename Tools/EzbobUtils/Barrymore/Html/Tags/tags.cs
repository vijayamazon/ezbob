namespace Ezbob.Utils.Html.Tags {
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
} // namespace
