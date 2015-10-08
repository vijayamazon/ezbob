namespace EzBob.Web.Infrastructure.Html {
	public enum EzButtonType {
		link,
		submit,
		button,
		input,
	} // enum EzButtonType

	public class EzButtonModel {
		public string Id { get; set; }
		public string Caption { get; set; }
		public string Cls { get; set; }
		public string UiEventControlID { get; set; }

		public string Href { get; set; }
		public int TabIndex { get; set; }
		public string Target { get; set; }
		public string OnClick { get; set; }

		public EzButtonType ButtonType { get; set; }
		public EzButtonModel() { }

		public EzButtonModel(string id, string caption,
			string cls = "",
			string uiEventControlID = "",
			int tabIndex = 0,
			EzButtonType buttonType = EzButtonType.button,
			string href = "",
			string target = "",
			string onClick = "") {
			Id = id;
			Caption = caption;
			Cls = cls;
			UiEventControlID = uiEventControlID;
			ButtonType = buttonType;
			TabIndex = tabIndex;
			Href = href;
			Target = target;
			OnClick = onClick;
		}//constructor
	}//EzButtonModel
}//ns