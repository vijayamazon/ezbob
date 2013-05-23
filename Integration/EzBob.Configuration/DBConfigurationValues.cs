using EZBob.DatabaseLib.Model;
using StructureMap;

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
		CollectionsCharge,
		BWABusinessCheck,
		CAISPath,
		CAISPath2,
		EnableAutomaticRejection,
		DisplayEarnedPoints
	} // enum Variables

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

		#region properties

		public bool DisplayEarnedPoints { get { return RawDisplayEarnedPoints == "1"; } }

		#endregion properties

		#region raw properties

		public string RawCollectionPeriod1 { get { return this[Variables.CollectionPeriod1]; } }
		public string RawCollectionPeriod2 { get { return this[Variables.CollectionPeriod2]; } }
		public string RawCollectionPeriod3 { get { return this[Variables.CollectionPeriod3]; } }
		public string RawLatePaymentCharge { get { return this[Variables.LatePaymentCharge]; } }
		public string RawRolloverCharge { get { return this[Variables.RolloverCharge]; } }
		public string RawPartialPaymentCharge { get { return this[Variables.PartialPaymentCharge]; } }
		public string RawAdministrationCharge { get { return this[Variables.AdministrationCharge]; } }
		public string RawCollectionsCharge { get { return this[Variables.CollectionsCharge]; } }
		public string RawBWABusinessCheck { get { return this[Variables.BWABusinessCheck]; } }
		public string RawCAISPath { get { return this[Variables.CAISPath]; } }
		public string RawCAISPath2 { get { return this[Variables.CAISPath2]; } }
		public string RawEnableAutomaticRejection { get { return this[Variables.EnableAutomaticRejection]; } }
		public string RawDisplayEarnedPoints { get { return this[Variables.DisplayEarnedPoints]; } }

		#endregion raw properties

		#region indexer

		public string this[Variables v] {
			get {
				var rep = ObjectFactory.GetInstance<IConfigurationVariablesRepository>();

				if (rep == null)
					return null;

				var oCfgValue = rep.GetByName(v.ToString());

				if (oCfgValue == null)
					return null;

				return oCfgValue.Value;
			}
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
