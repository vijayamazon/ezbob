namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Xml;
	using Database;
	using Logger;
	using Utils;

	[DataContract]
	[DL65]
	public class ExperianLtdDL65 : AExperianLtdDataRow {
		public ExperianLtdDL65(XmlNode oRoot = null, ASafeLog oLog = null) : base(oRoot, oLog) {
			ExperianLtdDL65ID = 0;
		} // constructor

		[DataMember]
		[NonTraversable]
		public long ExperianLtdDL65ID { get; set; }

		#region properties loaded from XML

		[DataMember]
		[DL65("CHARGENUMBER")]
		public string ChargeNumber { get; set; }
		[DataMember]
		[DL65("FORMNUMBERFLAG")]
		public string FormNumber { get; set; }
		[DataMember]
		[DL65("CURRENCYINDICATOR")]
		public string CurrencyIndicator { get; set; }
		[DataMember]
		[DL65("TOTAMTDEBENTURESECD")]
		public string TotalAmountOfDebentureSecured { get; set; }
		[DataMember]
		[DL65("CHARGETYPE")]
		public string ChargeType { get; set; }
		[DataMember]
		[DL65("AMTSECURED")]
		public string AmountSecured { get; set; }
		[DataMember]
		[DL65("PROPERTYDETAILS")]
		public string PropertyDetails { get; set; }
		[DataMember]
		[DL65("CHARGEETEXT")]
		public string ChargeeText { get; set; }
		[DataMember]
		[DL65("RESTRICTINGPROVNS")]
		public string RestrictingProvisions { get; set; }
		[DataMember]
		[DL65("REGULATINGPROVNS")]
		public string RegulatingProvisions { get; set; }
		[DataMember]
		[DL65("ALTERATIONSTOORDER")]
		public string AlterationsToTheOrder { get; set; }
		[DataMember]
		[DL65("PROPERTYRELDFROMCHGE")]
		public string PropertyReleasedFromTheCharge { get; set; }
		[DataMember]
		[DL65("AMTCHARGEINCRD")]
		public string AmountChargeIncreased { get; set; }
		[DataMember]
		[DL65("CREATIONDATE-YYYY")]
		public DateTime? CreationDate { get; set; }
		[DataMember]
		[DL65("DATEFULLYSATD-YYYY")]
		public DateTime? DateFullySatisfied { get; set; }
		[DataMember]
		[DL65("FULLYSATDINDICATOR")]
		public string FullySatisfiedIndicator { get; set; }
		[DataMember]
		[DL65("NUMPARTIALSATNDATES")]
		public int? NumberOfPartialSatisfactionDates { get; set; }
		[DataMember]
		[DL65("NUMPARTIALSATNDATAITEMS")]
		public int? NumberOfPartialSatisfactionDataItems { get; set; }

		#endregion properties loaded from XML

		#region protected

		#region method DoBeforeTheMainInsert

		protected override void DoBeforeTheMainInsert(List<string> oProcSql) {
			oProcSql.Add("\tDECLARE @ExperianLtdDL65ID INT");

			oProcSql.Add("\tDECLARE @c INT\n");
			oProcSql.Add("\tSELECT @c = COUNT(*) FROM @Tbl\n");
			oProcSql.Add("\tIF @c != 1");
			oProcSql.Add("\t\tRAISERROR('Invalid argument: no/too much data to insert into ExperianLtdDL65 table.', 11, 1)\n");
		} // DoBeforeTheMainInsert

		#endregion method DoBeforeTheMainInsert

		#region method DoAfterTheMainInsert

		protected override void DoAfterTheMainInsert(List<string> oProcSql) {
			oProcSql.Add("\n\tSET @ExperianLtdDL65ID = SCOPE_IDENTITY()\n");
			oProcSql.Add("\tSELECT @ExperianLtdDL65ID AS ExperianLtdID");
		} // DoAfterTheMainInsert

		#endregion method DoAfterTheMainInsert

		#region method SelfSave

		protected override bool SelfSave(AConnection oDB, ConnectionWrapper oPersistent) {
			try {
				ExperianLtdDL65ID = oDB.ExecuteScalar<long>(
					oPersistent,
					DBSaveProcName,
					CommandSpecies.StoredProcedure,
					oDB.CreateTableParameter(
						this.GetType(),
						"@Tbl",
						new List<ExperianLtdDL65> { this },
						TypeUtils.GetConvertorToObjectArray(this.GetType()),
						GetDBColumnTypes()
						)
					);
			}
			catch (Exception e) {
				Log.Warn(e, "Failed to save {0} to DB.", this.GetType().Name);
				ExperianLtdDL65ID = 0;
			} // try

			return ExperianLtdDL65ID > 0;
		} // SelfSave

		#endregion method SelfSave

		#region method GetID

		protected override long GetID() {
			return ExperianLtdDL65ID;
		} // GetParentID

		#endregion method GetID

		#region method LoadChildrenFromXml

		protected override void LoadChildrenFromXml() {
			LoadOneChildFromXml(typeof(ExperianLtdLenderDetails), null);
		} // LoadChildrenFromXml

		#endregion method LoadChildrenFromXml

		#endregion protected
	} // class ExperianLtdDL65
} // namespace
