namespace Ezbob.Utils.Html.Tags {
	using Ezbob.Utils.Html.Attributes;

	public class Img : ATag {
		public Img() { Src = new Src(); }
		public override bool MustClose { get { return false; } }
		public override string Tag { get { return "img"; } }
		public virtual Src Src { get; private set; }
	} // class Img
} // namespace
