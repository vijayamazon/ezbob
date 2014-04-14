namespace ConfigManager {
	using System;
	using System.Globalization;
	using Ezbob.Logger;

	public class VariableValue {
		#region static constructor

		static VariableValue() {
			ms_oLock = new object();
			LogVerbosityLevel = LogVerbosityLevel.Compact;
		} // static constructor

		#endregion static constructor

		#region property LogVerbosityLevel

		public static LogVerbosityLevel LogVerbosityLevel {
			get {
				LogVerbosityLevel nLevel;

				lock (ms_oLock) {
					nLevel = ms_nLogVerbosityLevel;
				} // lock

				return nLevel;
			} // get

			set {
				lock (ms_oLock) {
					ms_nLogVerbosityLevel = value;
				} // lock
			} // set
		} // LogVerbosityLevel

		private static LogVerbosityLevel ms_nLogVerbosityLevel;

		#endregion property LogVerbosityLevel

		#region type case operators

		#region operator string

		public static implicit operator string(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("String configuration variable not specified.");

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as string: '{1}'", oValue.Name, oValue.Value);

			return oValue.Value;
		} // operator to string

		#endregion operator string

		#region operator int

		public static implicit operator int(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Integer configuration variable not specified.");

			int nValue = Convert.ToInt32(oValue.Value, CultureInfo.InvariantCulture);

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as int: '{1}' -> {2}", oValue.Name, oValue.Value, nValue);

			return nValue;
		} // operator to int

		#endregion operator int

		#region operator long

		public static implicit operator long(VariableValue oValue)
		{
			if (oValue == null)
				throw new NullReferenceException("Long configuration variable not specified.");

			long nValue = Convert.ToInt64(oValue.Value, CultureInfo.InvariantCulture);

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as int: '{1}' -> {2}", oValue.Name, oValue.Value, nValue);

			return nValue;
		} // operator to long

		#endregion operator long

		#region operator double

		public static implicit operator double(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Double configuration variable not specified.");

			double nValue = Convert.ToDouble(oValue.Value, CultureInfo.InvariantCulture);

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as double: '{1}' -> {2}", oValue.Name, oValue.Value, nValue);

			return nValue;
		} // operator to double

		#endregion operator double

		#region operator decimal

		public static implicit operator decimal(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Decimal configuration variable not specified.");

			decimal nValue = Convert.ToDecimal(oValue.Value, CultureInfo.InvariantCulture);

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as decimal: '{1}' -> {2}", oValue.Name, oValue.Value, nValue);

			return nValue;
		} // operator to decimal

		#endregion operator decimal

		#region operator bool

		public static implicit operator bool(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Boolean configuration variable not specified.");

			bool bValue = false;

			switch (oValue.Value.ToLower(CultureInfo.InvariantCulture)) {
			case "true":
			case "1":
			case "yes":
				bValue = true;
				break;
			} // switch

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as boolean: '{1}' -> {2}", oValue.Name, oValue.Value, bValue ? "true" : "false");

			return bValue;
		} // operator to bool

		#endregion operator bool

		#endregion type case operators

		#region constructor

		public VariableValue(Variables nName, string sValue, ASafeLog oLog) {
			Name = nName;
			Value = sValue;

			m_oLog = oLog ?? new SafeLog();
		} // constructor

		#endregion constructor

		#region property Name

		public virtual Variables Name { get; protected set; }

		#endregion property Name

		#region property Value

		public virtual string Value {
			get { return m_sValue; } // get
			protected set { m_sValue = value ?? ""; } // set
		} // Value

		private string m_sValue;

		#endregion property Value

		#region private

		private readonly ASafeLog m_oLog;

		private static readonly object ms_oLock;

		#endregion private
	} // class VariableValue
} // namespace ConfigManager
