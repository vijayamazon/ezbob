namespace ConfigManager {
	using System;
	using System.Globalization;

	public class VariableValue {
		#region type case operators

		#region operator string

		public static implicit operator string(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("String configuration variable not specified.");

			return oValue.Value;
		} // operator to string

		#endregion operator string

		#region operator int

		public static implicit operator int(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Integer configuration variable not specified.");

			return Convert.ToInt32(oValue.Value, CultureInfo.InvariantCulture);
		} // operator to int

		#endregion operator int

		#region operator double

		public static implicit operator double(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Double configuration variable not specified.");

			return Convert.ToDouble(oValue.Value, CultureInfo.InvariantCulture);
		} // operator to double

		#endregion operator double

		#region operator decimal

		public static implicit operator decimal(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Decimal configuration variable not specified.");

			return Convert.ToDecimal(oValue.Value, CultureInfo.InvariantCulture);
		} // operator to decimal

		#endregion operator decimal

		#region operator bool

		public static implicit operator bool(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Boolean configuration variable not specified.");

			switch (oValue.Value.ToLower(CultureInfo.InvariantCulture)) {
			case "true":
			case "1":
			case "yes":
				return true;
			} // switch

			return false;
		} // operator to bool

		#endregion operator bool

		#endregion type case operators

		#region constructor

		public VariableValue(string sValue) {
			Value = sValue;
		} // constructor

		#endregion constructor

		#region property Value

		public virtual string Value {
			get { return m_sValue; } // get
			protected set { m_sValue = value ?? ""; } // set
		} // Value

		private string m_sValue;

		#endregion property Value
	} // class VariableValue
} // namespace ConfigManager
