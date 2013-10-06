using EZBob.DatabaseLib;
using EzBob.CommonLib;
using StructureMap;
using log4net;

namespace EzBob.Configuration {
	#region enum variables

	public enum Variables {
		CollectionPeriod1,
		CollectionPeriod2,
		CollectionPeriod3,
		LatePaymentCharge,
		RolloverCharge,
		PartialPaymentCharge,
		AdministrationCharge,
		BWABusinessCheck,
		CAISPath,
		CAISPath2,
		EnableAutomaticRejection,
		DisplayEarnedPoints,
		ChannelGrabberRejectPolicy,
		CompanyScoreParserConfiguration,
		DirectorInfoParserConfiguration,
	} // enum Variables

	public enum ChannelGrabberRejectPolicy {
		Never,
		ConnectionFail,
	} // enum ChannelGrabberRejectPolicy

	#endregion enum variables

	#region class DBConfigurationValues

	public class DBConfigurationValues {
		#region public

		#region Instance

		public static DBConfigurationValues Instance {
			get {
				if (ms_oInstance == null)
					ms_oInstance = new DBConfigurationValues();

				return ms_oInstance;
			}
		} // Instance

		private static DBConfigurationValues ms_oInstance;

		#endregion Instance

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(DBConfigurationValues));

		#region properties

		public bool DisplayEarnedPoints { get { return RawDisplayEarnedPoints == "1"; } }

		public ChannelGrabberRejectPolicy ChannelGrabberRejectPolicy {
			get {
				ChannelGrabberRejectPolicy cgrp = ChannelGrabberRejectPolicy.Never;
				ChannelGrabberRejectPolicy.TryParse(RawChannelGrabberRejectPolicy, true, out cgrp);
				return cgrp;
			} // get
		} // ChannelGrabberRejectPolicy

		public string CompanyScoreParserConfiguration { get { return RawCompanyScoreParserConfiguration; } }
		public string DirectorInfoParserConfiguration { get { return RawDirectorInfoParserConfiguration; } }

		#endregion properties

		#region raw properties

		public string RawCollectionPeriod1 { get { return this[Variables.CollectionPeriod1]; } }
		public string RawCollectionPeriod2 { get { return this[Variables.CollectionPeriod2]; } }
		public string RawCollectionPeriod3 { get { return this[Variables.CollectionPeriod3]; } }
		public string RawLatePaymentCharge { get { return this[Variables.LatePaymentCharge]; } }
		public string RawRolloverCharge { get { return this[Variables.RolloverCharge]; } }
		public string RawPartialPaymentCharge { get { return this[Variables.PartialPaymentCharge]; } }
		public string RawAdministrationCharge { get { return this[Variables.AdministrationCharge]; } }
		public string RawBWABusinessCheck { get { return this[Variables.BWABusinessCheck]; } }
		public string RawCAISPath { get { return this[Variables.CAISPath]; } }
		public string RawCAISPath2 { get { return this[Variables.CAISPath2]; } }
		public string RawEnableAutomaticRejection { get { return this[Variables.EnableAutomaticRejection]; } }
		public string RawDisplayEarnedPoints { get { return this[Variables.DisplayEarnedPoints]; } }
		public string RawChannelGrabberRejectPolicy { get { return this[Variables.ChannelGrabberRejectPolicy]; } }
		public string RawCompanyScoreParserConfiguration { get { return this[Variables.CompanyScoreParserConfiguration]; } }
		public string RawDirectorInfoParserConfiguration { get { return this[Variables.DirectorInfoParserConfiguration]; } }

		#endregion raw properties

		#region indexer

		public string this[Variables v] {
			get {
				var oDBHelper = ObjectFactory.GetInstance<IDatabaseDataHelper>() as DatabaseDataHelper;

				if (oDBHelper == null) {
					ms_oLog.Error("Failed to retrieve database helper.");
					return null;
				} // if

				var rep = oDBHelper.ConfigurationVariables;

				if (rep == null) {
					ms_oLog.Error("Failed to retrieve configuration variables.");
					return null;
				} // if

				var oCfgValue = rep.GetByName(v.ToString());

				if (oCfgValue == null) {
					ms_oLog.WarnFormat("Configuration variable not found: {0}", v.ToString());
					return null;
				} // if

				return oCfgValue.Value;
			} // get
		} // indexer

		#endregion indexer

		#endregion public

		#region private

		#region constructor

		private DBConfigurationValues() {
		} // constructor

		#endregion constructor

		#endregion private
	} // class DBConfigurationValues

	#endregion class DBConfigurationValues
} // namespace EzBob.Configuration
