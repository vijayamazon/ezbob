namespace EzServiceCrontab.ArgumentTypes {
	using System;
	using System.ComponentModel;

	internal class Enumeration : AType<System.Enum> {
		#region constructor

		public Enumeration() : base("enum") {
			m_oUnderlyingType = null;
		} // constructor

		#endregion constructor

		#region property FullName

		public override string FullName {
			get { return Name + "(" + Hint + ")" + (CanBeNull ? "?" : string.Empty); }
		} // FullName

		#endregion property FullName

		#region property CanBeNull

		public override bool CanBeNull {
			get { return false; }
		} // CanBeNull

		#endregion property CanBeNull

		#region property UnderlyingType

		public override Type UnderlyingType {
			get {
				return m_oUnderlyingType;
			} // get
		} // UnderlyingType

		private Type m_oUnderlyingType;

		#endregion property UnderlyingType

		#region method OnHintUpdated

		protected override void OnHintUpdated() {
			if (string.IsNullOrWhiteSpace(Hint))
				throw new InvalidEnumArgumentException("Enum name not specified for enum type.");

			m_oUnderlyingType = FindType(Hint);

			if (m_oUnderlyingType == null)
				throw new InvalidEnumArgumentException("Enum name was not found from '" + Hint + "'.");
		} // OnHintUpdated

		#endregion method OnHintUpdated

		#region method Clone

		public override IType Clone() {
			IType oResult = (IType)Activator.CreateInstance(this.GetType());

			if (Hint != string.Empty)
				oResult.Hint = Hint;

			return oResult;
		} // Clone

		#endregion method Clone

		#region method CreateInstance

		public override object CreateInstance(string sValue) {
			try {
				return Enum.Parse(UnderlyingType, sValue, true);
			}
			catch (Exception e) {
				throw GetError(sValue, e);
			} // try
		} // CreateInstance

		#endregion method CreateInstance

		#region method ToString

		public override string ToString() {
			return string.Format("{0} as {1}", FullName, m_oUnderlyingType != null ? UnderlyingType.ToString() : "<Enum yet to be detected>");
		} // ToString

		#endregion method ToString
	} // class Enumeration
} // namespace
