namespace Ezbob.Utils.Html.Tags {
	using Ezbob.Utils.Html.Attributes;

	public class A : ATag {
		public A() { Href = new Href(); Title = new Title(); Alt = new Alt(); Target = new Target(); }
		public override string Tag { get { return "a"; } }
		public virtual Alt Alt { get; private set; }
		public virtual Href Href { get; private set; }
		public virtual Target Target { get; private set; }
		public virtual Title Title { get; private set; }
	} // class A
} // namespace
