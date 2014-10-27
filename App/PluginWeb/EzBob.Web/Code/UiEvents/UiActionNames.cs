namespace EzBob.Web.Code.UiEvents {
	public enum UiActionNames {
		Click,
		Change,
		Checked,
		Linked,
		FocusIn,
		FocusOut,
		SlideStart,
		SlideStop,
		Slide,
		PageLoad,
	} // enum UiActionNames

	public static class UiActionNamesExt {
		public static string ToDBName(this UiActionNames nName) {
			return nName.ToString().ToLowerInvariant();
		} // ToDBName
	} // class UiActionNamesExt
} // namespace
