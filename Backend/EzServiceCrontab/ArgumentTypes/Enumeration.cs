namespace EzServiceCrontab.ArgumentTypes {
	using System;
	using System.ComponentModel;
	using Ezbob.Utils;

	internal class Enumeration : AType<System.Enum> {

		public Enumeration() : base("enum") {
			m_oUnderlyingType = null;
		} // constructor

		public override string FullName {
			get { return Name + "(" + Hint + ")" + (CanBeNull ? "?" : string.Empty); }
		} // FullName

		public override bool CanBeNull {
			get { return false; }
		} // CanBeNull

		public override Type UnderlyingType {
			get {
				return m_oUnderlyingType;
			} // get
		} // UnderlyingType

		private Type m_oUnderlyingType;

		protected override void OnHintUpdated() {
			if (string.IsNullOrWhiteSpace(Hint))
				throw new InvalidEnumArgumentException("Enum name not specified for enum type.");

			m_oUnderlyingType = TypeUtils.FindType(Hint);

			if (m_oUnderlyingType == null)
				throw new InvalidEnumArgumentException("Enum name was not found from '" + Hint + "'.");
		} // OnHintUpdated

		public override IType Clone() {
			IType oResult = (IType)Activator.CreateInstance(this.GetType());

			if (Hint != string.Empty)
				oResult.Hint = Hint;

			return oResult;
		} // Clone

		public override object CreateInstance(string sValue) {
			try {
				return Enum.Parse(UnderlyingType, sValue, true);
			}
			catch (Exception e) {
				throw GetError(sValue, e);
			} // try
		} // CreateInstance

		public override string ToString() {
			return string.Format("{0} as {1}", FullName, m_oUnderlyingType != null ? UnderlyingType.ToString() : "<Enum yet to be detected>");
		} // ToString

	} // class Enumeration
} // namespace
