namespace Ezbob.Utils.Html.Tags {
	using Ezbob.Utils.Html.Attributes;

	public abstract class ATableCell : ATag {
		public virtual Colspan Colspan { get; private set; }
		public virtual Rowspan Rowspan { get; private set; }
		public virtual Title Title { get; private set; }

		protected ATableCell() {
			Colspan = new Colspan();
			Rowspan = new Rowspan();
			Title = new Title();
		} // constructor
	} // class ATableCell
} // namespace
