namespace Ezbob.Backend.ModelsWithDB.Experian.Attributes {
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json;

	public enum TransformationType {
		None,
		Money,
		MonthsAndYears,
		Shares,
	} // TransformationType

	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
	public abstract class ASrcAttribute : Attribute {
		public string GroupName { get; private set; }

		public string NodeName { get; private set; }

		public virtual bool IsTopLevel { get { return true; } }

		public virtual bool IsCompanyScoreModel {
			get { return m_bIsCompanyScoreModel; }
			set { m_bIsCompanyScoreModel = value; }
		} // IsCompanyScoreModel

		private bool m_bIsCompanyScoreModel;

		public string GroupPath {
			get { return "." + (IsTopLevel ? "/REQUEST" : "") + "/" + GroupName; }
		} // GroupPath

		public string NodePath {
			get { return GroupPath + "/" + NodeName; }
		} // GroupPath

		public virtual DisplayMetaData MetaData { get { return null; } }

		public virtual string TargetDisplayGroup { get { return null; } }

		public virtual string TargetDisplayName { get; private set; }

		public virtual int TargetDisplayPosition { get { return 0; } }

		public virtual TransformationType Transformation {
			get { return m_nTransformation; }
			set { m_nTransformation = value; }
		} // Transformation

		private TransformationType m_nTransformation;

		public virtual string DisplayPrefix {
			get { return m_sDisplayPrefix; }
			set { m_sDisplayPrefix = value; }
		} // DisplayPrefix

		private string m_sDisplayPrefix;

		public override string ToString() {
			return string.IsNullOrWhiteSpace(NodeName) ? GroupPath : NodePath;
		} // ToString

		public virtual string Map(string sValue) {
			if ((m_oMap == null) || !m_oMap.ContainsKey(sValue))
				return sValue;

			return sValue + " - " + m_oMap[sValue];
		} // Map

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

		private readonly SortedDictionary<string, string> m_oMap;

	} // class ASrcAttribute

} // namespace
