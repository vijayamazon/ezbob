namespace ConfigManager {
	using System;
	using System.Diagnostics;
	using System.Globalization;
	using Ezbob.Logger;

	public class VariableValue {

		static VariableValue() {
			ms_oLock = new object();
			LogVerbosityLevel = LogVerbosityLevel.Compact;
		} // static constructor

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

		public static implicit operator string(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("String configuration variable not specified that is used at " + GetPosition());

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as string: '{1}'", oValue.Name, oValue.Value);

			return oValue.Value;
		} // operator to string

		public static implicit operator int(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Integer configuration variable not specified that is used at " + GetPosition());

			int nValue = Convert.ToInt32(oValue.Value, CultureInfo.InvariantCulture);

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as int: '{1}' -> {2}", oValue.Name, oValue.Value, nValue);

			return nValue;
		} // operator to int

		public static implicit operator long(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Long configuration variable not specified that is used at " + GetPosition());

			long nValue = Convert.ToInt64(oValue.Value, CultureInfo.InvariantCulture);

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as long: '{1}' -> {2}", oValue.Name, oValue.Value, nValue);

			return nValue;
		} // operator to long

		public static implicit operator ulong(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Long configuration variable not specified that is used at " + GetPosition());

			ulong nValue = Convert.ToUInt64(oValue.Value, CultureInfo.InvariantCulture);

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as ulong: '{1}' -> {2}", oValue.Name, oValue.Value, nValue);

			return nValue;
		} // operator to ulong

		public static implicit operator double(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Double configuration variable not specified that is used at " + GetPosition());

			double nValue = Convert.ToDouble(oValue.Value, CultureInfo.InvariantCulture);

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as double: '{1}' -> {2}", oValue.Name, oValue.Value, nValue);

			return nValue;
		} // operator to double

		public static implicit operator decimal(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Decimal configuration variable not specified that is used at " + GetPosition());

			decimal nValue = Convert.ToDecimal(oValue.Value, CultureInfo.InvariantCulture);

			if (LogVerbosityLevel == LogVerbosityLevel.Verbose)
				oValue.m_oLog.Debug("VariableValue {0} requested as decimal: '{1}' -> {2}", oValue.Name, oValue.Value, nValue);

			return nValue;
		} // operator to decimal

		public static implicit operator bool(VariableValue oValue) {
			if (oValue == null)
				throw new NullReferenceException("Boolean configuration variable not specified that is used at " + GetPosition());

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

		public VariableValue(int nID, Variables nName, string sValue, string sDescription, ASafeLog oLog) {
			ID = nID;
			Name = nName;
			Value = sValue;
			Description = sDescription;

			m_oLog = oLog ?? new SafeLog();
		} // constructor

		public VariableValue Update(string sValue) {
			Value = sValue;
			return this;
		} // Update

		public virtual int ID { get; private set; }

		public virtual Variables Name { get; private set; }

		public virtual string Value {
			get { return m_sValue ?? ""; } // get
			private set { m_sValue = value ?? ""; } // set
		} // Value

		private string m_sValue;

		public virtual string Description { get; private set; } // Description

		private readonly ASafeLog m_oLog;

		private static readonly object ms_oLock;

		private static string GetPosition() {
			var oStack = new StackTrace(true);

			if (oStack.FrameCount <= 2)
				return "unknown location";

			StackFrame oFrame = oStack.GetFrame(2);

			var oMethod = oFrame.GetMethod();

			string sMethodName = oMethod == null ? string.Empty : " in " + oMethod.Name;

			return string.Format("{0}:{1},{2}{3}", oFrame.GetFileName(), oFrame.GetFileLineNumber(), oFrame.GetFileColumnNumber(), sMethodName);
		} // GetPosition

	} // class VariableValue
} // namespace ConfigManager
