namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System;
	using System.Collections.Generic;
	using System.Runtime.Serialization;
	using System.Xml;
	using Attributes;
	using Database;
	using Logger;
	using Utils;

	[DataContract]
	[DL65]
	public class ExperianLtdDL65 : AExperianLtdDataRow {
		public ExperianLtdDL65(ASafeLog oLog = null) : base(oLog) {
			ExperianLtdDL65ID = 0;
		} // constructor

		[DataMember]
		[NonTraversable]
		public long ExperianLtdDL65ID { get; set; }

		[DataMember]
		[DL65("CHARGENUMBER", "Charge Number")]
		public string ChargeNumber { get; set; }

		[DataMember]
		[DL65("FORMNUMBERFLAG", "Form Number", @"{
			""04"": ""'A' - Mortgage extracted from credit master file: All charges until autumn 1994"",
			""08"": ""'B' - Satisfaction of a mortgage originally extracted from the credit master file as record type 'A'"",
			""12"": ""'9999' - Correction to charge details"",
			""16"": ""'395' - Mortgage or charge registration"",
			""20"": ""'400' - Mortgage or charge subject to which property has been acquired"",
			""24"": ""'397' - Charge securing a series of debentures"",
			""28"": ""'397A' - Issue of secured debentures in a series"",
			""32"": ""'403A' - Satisfaction in full or part of a charge"",
			""36"": ""'403B' - Part of property or undertaking charged has been released from the charge or no longer part of the company's property/undertaking"",
			""40"": ""'4051' - Appointment of receiver or manager"",
			""44"": ""'4052' - Cessation of receiver or manager"",
			""48"": ""'410' - Scottish mortgage/charge registration"",
			""52"": ""'413' - Charge securing a series of debentures"",
			""56"": ""'413A' - Issue of secured debentures in a series"",
			""60"": ""'416' - Mortgage or charge subject to which property has been acquired"",
			""64"": ""'419A' - Satisfaction in full or part of a mortgage or charge"",
			""68"": ""'419B' - Declaration that part of the property or undertaking has been released from the charge or no longer forms part of the company's property or undertaking"",
			""72"": ""'1SC' - Appointment of a receiver by the holder of a floating charge"",
			""76"": ""'2SC' - Appointment of a receiver by the court"",
			""80"": ""'3SC' - Cessation of a receiver"",
			""84"": ""'33SC' - Death of a receiver"",
			""88"": ""'466' - Instrument of alteration to a floating charge""
		}")]
		public string FormNumber { get; set; }

		[DataMember]
		[DL65("CURRENCYINDICATOR", "Currency Indicator")]
		public string CurrencyIndicator { get; set; }

		[DataMember]
		[DL65("TOTAMTDEBENTURESECD", "Total Amount of Debenture Secured")]
		public string TotalAmountOfDebentureSecured { get; set; }

		[DataMember]
		[DL65("CHARGETYPE", "Charge type")]
		public string ChargeType { get; set; }

		[DataMember]
		[DL65("AMTSECURED", "Amount secured")]
		public string AmountSecured { get; set; }

		[DataMember]
		[DL65("PROPERTYDETAILS", "Property details")]
		public string PropertyDetails { get; set; }

		[DataMember]
		[DL65("CHARGEETEXT", "Chargee text")]
		public string ChargeeText { get; set; }

		[DataMember]
		[DL65("RESTRICTINGPROVNS", "Restricting provisions")]
		public string RestrictingProvisions { get; set; }

		[DataMember]
		[DL65("REGULATINGPROVNS", "Regulating provisions")]
		public string RegulatingProvisions { get; set; }

		[DataMember]
		[DL65("ALTERATIONSTOORDER", "Alterations to the order")]
		public string AlterationsToTheOrder { get; set; }

		[DataMember]
		[DL65("PROPERTYRELDFROMCHGE", "Property released from the charge")]
		public string PropertyReleasedFromTheCharge { get; set; }

		[DataMember]
		[DL65("AMTCHARGEINCRD", "Amount charge increased")]
		public string AmountChargeIncreased { get; set; }

		[DataMember]
		[DL65("CREATIONDATE-YYYY", "Creation date")]
		public DateTime? CreationDate { get; set; }

		[DataMember]
		[DL65("DATEFULLYSATD-YYYY", "Date fully satisfied")]
		public DateTime? DateFullySatisfied { get; set; }

		[DataMember]
		[DL65("FULLYSATDINDICATOR", "Fully satisfied indicator", @"{
			""F"": ""Fully satisfied"",
			""N"": ""Not satisfied""
		}")]
		public string FullySatisfiedIndicator { get; set; }

		[DataMember]
		[DL65("NUMPARTIALSATNDATES", "Number of partial satisfaction dates")]
		public int? NumberOfPartialSatisfactionDates { get; set; }

		[DataMember]
		[DL65("NUMPARTIALSATNDATAITEMS", "Number of partial satisfaction data items")]
		public int? NumberOfPartialSatisfactionDataItems { get; set; }

		[DataMember]
		[NonTraversable]
		public override long ID {
			get { return ExperianLtdDL65ID; }
			set { ExperianLtdDL65ID = value; }
		} // ID

		protected override void DoBeforeTheMainInsert(List<string> oProcSql) {
			oProcSql.Add("\tDECLARE @ExperianLtdDL65ID INT");

			oProcSql.Add("\tDECLARE @c INT\n");
			oProcSql.Add("\tSELECT @c = COUNT(*) FROM @Tbl\n");
			oProcSql.Add("\tIF @c != 1");
			oProcSql.Add("\t\tRAISERROR('Invalid argument: no/too much data to insert into ExperianLtdDL65 table.', 11, 1)\n");
		} // DoBeforeTheMainInsert

		protected override void DoAfterTheMainInsert(List<string> oProcSql) {
			oProcSql.Add("\n\tSET @ExperianLtdDL65ID = SCOPE_IDENTITY()\n");
			oProcSql.Add("\tSELECT @ExperianLtdDL65ID AS ExperianLtdID");
		} // DoAfterTheMainInsert

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

		protected override void LoadChildrenFromXml(XmlNode oRoot) {
			LoadOneChildFromXml(oRoot, typeof(ExperianLtdLenderDetails), null);
		} // LoadChildrenFromXml

	} // class ExperianLtdDL65
} // namespace
