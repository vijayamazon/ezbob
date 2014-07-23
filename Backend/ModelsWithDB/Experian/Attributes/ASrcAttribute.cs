namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	#region enum TransformationType

	public enum TransformationType {
		None,
		Money,
		MonthsAndYears,
		Shares,
	} // TransformationType

	#endregion enum TransformationType

	#region class ASrcAttribute

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public abstract class ASrcAttribute : Attribute {
		public string GroupName { get; private set; }

		public string NodeName { get; private set; }

		public virtual bool IsTopLevel { get { return true; } }

		#region property IsCompanyScoreModel

		public virtual bool IsCompanyScoreModel {
			get { return m_bIsCompanyScoreModel; }
			set { m_bIsCompanyScoreModel = value; }
		} // IsCompanyScoreModel

		private bool m_bIsCompanyScoreModel;

		#endregion property IsCompanyScoreModel

		#region property GroupPath

		public string GroupPath {
			get { return "." + (IsTopLevel ? "/REQUEST" : "") + "/" + GroupName; }
		} // GroupPath

		#endregion property GroupPath

		#region property NodePath

		public string NodePath {
			get { return GroupPath + "/" + NodeName; }
		} // GroupPath

		#endregion property NodePath

		public virtual DisplayMetaData MetaData { get { return null; } }

		public virtual string TargetDisplayGroup { get { return null; } }

		public virtual string TargetDisplayName { get; private set; }

		public virtual int TargetDisplayPosition { get { return 0; } }

		#region property Transformation

		public virtual TransformationType Transformation {
			get { return m_nTransformation; }
			set { m_nTransformation = value; }
		} // Transformation

		private TransformationType m_nTransformation;

		#endregion property Transformation

		#region property DisplayPrefix

		public virtual string DisplayPrefix {
			get { return m_sDisplayPrefix; }
			set { m_sDisplayPrefix = value; }
		} // DisplayPrefix

		private string m_sDisplayPrefix;

		#endregion property DisplayPrefix

		#region method ToString

		public override string ToString() {
			return string.IsNullOrWhiteSpace(NodeName) ? GroupPath : NodePath;
		} // ToString

		#endregion method ToString

		#region method Map

		public virtual string Map(string sValue) {
			if ((m_oMap == null) || !m_oMap.ContainsKey(sValue))
				return sValue;

			return sValue + " - " + m_oMap[sValue];
		} // Map

		#endregion method Map

		#region protected

		protected ASrcAttribute(string sGroupName, string sNodeName, string sTargetDisplayName, string sMap) {
			GroupName = sGroupName;
			NodeName = sNodeName;
			TargetDisplayName = sTargetDisplayName;
			m_sDisplayPrefix = "\n";
			m_bIsCompanyScoreModel = true;
			m_nTransformation = TransformationType.None;

			if (!string.IsNullOrWhiteSpace(sMap)) {
				try {
					m_oMap = JsonConvert.DeserializeObject<SortedDictionary<string, string>>(sMap);
				}
				catch (Exception) {
					m_oMap = null;
				} // try
			}
			else
				m_oMap = null;
		} // constructor

		#endregion protected

		#region private

		private readonly SortedDictionary<string, string> m_oMap;

		#endregion private
	} // class ASrcAttribute

	#endregion class ASrcAttribute
} // namespace
