namespace EzBob.Backend.Strategies {
	using System;
	using System.Globalization;
	using ExperianLib;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianConsumerCheck : AStrategy {
		#region public

		#region constructor

		public ExperianConsumerCheck(int nCustomerID, int nDirectorID, bool bForceCheck, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID;
			m_nDirectorID = nDirectorID;
			m_bForceCheck = bForceCheck;

			m_oPersonalData = new GetPersonalInfoForConsumerCheck(m_nCustomerID, m_nDirectorID, DB, Log).FillFirst<GetPersonalInfoForConsumerCheck.ResultRow>();

			m_oAddressLines = new GetCustomerAddresses(m_nCustomerID, DB, Log).FillFirst<GetCustomerAddresses.ResultRow>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Experian consumer check"; }
		} // Name

		#endregion property Name

		#region property Score

		public int Score { get; private set; }

		#endregion property Score

		#region method Execute

		public override void Execute() {
			Log.Info("Starting consumer check with parameters: {0} {1}.", m_oPersonalData, m_oAddressLines);

			bool bSuccess = GetConsumerInfoAndSave(AddressCurrency.Current);

			if (!bSuccess && CanUsePrevAddress())
				bSuccess = GetConsumerInfoAndSave(AddressCurrency.Previous);

			Log.Info("Consumer check {2} with parameters: {0} {1}.", m_oPersonalData, m_oAddressLines, bSuccess ? "succeeded" : "failed");
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		#region method CanUsePrevAddress

		private bool CanUsePrevAddress() {
			return
				(m_nDirectorID == 0) &&
				(m_oPersonalData.TimeAtAddress == 1) &&
				!string.IsNullOrEmpty(m_oAddressLines.GetLine6(AddressCurrency.Previous));
		} // CanUsePrevAddress

		#endregion method CanUsePrevAddress

		#region method GetConsumerInfoAndSave

		private bool GetConsumerInfoAndSave(AddressCurrency oAddressCurrency) {
			InputLocationDetailsMultiLineLocation location = m_oAddressLines.GetLocation(oAddressCurrency);

			var consumerService = new ConsumerService();

			ConsumerServiceResult result = consumerService.GetConsumerInfo(
				m_oPersonalData.FirstName,
				m_oPersonalData.Surname,
				m_oPersonalData.Gender,
				m_oPersonalData.DateOfBirth,
				null,
				location,
				"PL",
				m_nCustomerID,
				m_nDirectorID,
				false,
				m_nDirectorID != 0,
				m_bForceCheck
			);

			if (!result.IsError)
				Score = (int)result.BureauScore;

			var sp = new UpdateExperianConsumer(DB, Log) {
				Name = m_oPersonalData.FirstName,
				Surname = m_oPersonalData.Surname,
				PostCode = m_oAddressLines.GetLine6(oAddressCurrency),
				ExperianError = result.Error,
				ExperianScore = Score,
				CustomerID = m_nCustomerID,
				DirectorID = m_nDirectorID,
			};

			sp.ExecuteNonQuery();

			return !result.IsError;
		} // GetConsumerInfoAndSave

		#endregion method GetConsumerInfoAndSave

		#region fields

		private readonly int m_nCustomerID;
		private readonly int m_nDirectorID;
		private readonly bool m_bForceCheck;

		private readonly GetCustomerAddresses.ResultRow m_oAddressLines;
		private readonly GetPersonalInfoForConsumerCheck.ResultRow m_oPersonalData;

		#endregion fields

		#region stored procedure classes
		// ReSharper disable MemberCanBePrivate.Local
		// ReSharper disable UnusedAutoPropertyAccessor.Local

		#region class GetPersonalInfoForConsumerCheck

		private class GetPersonalInfoForConsumerCheck : AStoredProcedure {
			public GetPersonalInfoForConsumerCheck(int nCustomerID, int nDirectorID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerID = nCustomerID;
				DirectorID = nDirectorID;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			public int CustomerID { get; set; }

			public int DirectorID { get; set; }

			public class ResultRow : AResultRow {
				public string FirstName { get; set; }
				public string Surname { get; set; }
				public string Gender { get; set; }
				public DateTime DateOfBirth { get; set; }
				public int TimeAtAddress { get; set; }

				public override string ToString() {
					return string.Format(
						"FirstName='{0}' Surname='{1}' Gender='{2}' DateOfBirth='{3}' Time at address={4}",
						FirstName, Surname, Gender, DateOfBirth.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture), TimeAtAddress
					);
				} // ToString
			} // class ResultRow
		} // class GetPersonalInfoForConsumerCheck

		#endregion class GetPersonalInfoForConsumerCheck

		#region class GetCustomerAddresses

		private enum AddressCurrency {
			Current,
			Previous,
		} // enum AddressCurrency

		private class GetCustomerAddresses : AStoredProcedure {
			public GetCustomerAddresses(int nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
				CustomerID = nCustomerID;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			public int CustomerID { get; set; }

			public class ResultRow : AResultRow {
				public string Line1 { get; set; }
				public string Line2 { get; set; }
				public string Line3 { get; set; }
				public string Line4 { get; set; }
				public string Line5 { get; set; }
				public string Line6 { get; set; }

				public string Line1Prev { get; set; }
				public string Line2Prev { get; set; }
				public string Line3Prev { get; set; }
				public string Line4Prev { get; set; }
				public string Line5Prev { get; set; }
				public string Line6Prev { get; set; }

				public override string ToString() {
					return string.Format(
						"Line1='{0}' Line2='{1}' Line3='{2}' Line4='{3}' Line5='{4}' Line6='{5}' " +
						"PrevLine1='{6}' PrevLine2='{7}' PrevLine3='{8}' PrevLine4='{9}' PrevLine5='{10}' PrevLine6='{11}'",
						Line1, Line2, Line3, Line4, Line5, Line6,
						Line1Prev, Line2Prev, Line3Prev, Line4Prev, Line5Prev, Line6Prev
					);
				} // ToString

				public InputLocationDetailsMultiLineLocation GetLocation(AddressCurrency oCurrency) {
					switch (oCurrency) {
					case AddressCurrency.Current:
						return new InputLocationDetailsMultiLineLocation {
							LocationLine1 = Line1,
							LocationLine2 = Line2,
							LocationLine3 = Line3,
							LocationLine4 = Line4,
							LocationLine5 = Line5,
							LocationLine6 = Line6,
						};

					case AddressCurrency.Previous:
						return new InputLocationDetailsMultiLineLocation {
							LocationLine1 = Line1Prev,
							LocationLine2 = Line2Prev,
							LocationLine3 = Line3Prev,
							LocationLine4 = Line4Prev,
							LocationLine5 = Line5Prev,
							LocationLine6 = Line6Prev,
						};

					default:
						throw new ArgumentOutOfRangeException("oCurrency", "Unsupported value: " + oCurrency.ToString());
					} // switch
				} // GetLocation

				public string GetLine6(AddressCurrency oCurrency) {
					switch (oCurrency) {
					case AddressCurrency.Current:
						return Line6;

					case AddressCurrency.Previous:
						return Line6Prev;

					default:
						throw new ArgumentOutOfRangeException("oCurrency", "Unsupported value: " + oCurrency.ToString());
					} // switch
				} // GetLocation
			} // class ResultRow
		} // class GetCustomerAddresses

		#endregion class GetCustomerAddresses

		#region class UpdateExperianConsumer

		private class UpdateExperianConsumer : AStoredProcedure {
			public UpdateExperianConsumer(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return CustomerID > 0;
			} // HasValidParameters

			public string Name { get; set; }
			public string Surname { get; set; }
			public string PostCode { get; set; }
			public string ExperianError { get; set; }
			public int ExperianScore { get; set; }
			public long CustomerID { get; set; }
			public long DirectorID { get; set; }
		} // class UpdateExperianConsumer

		#endregion class UpdateExperianConsumer

		// ReSharper restore UnusedAutoPropertyAccessor.Local
		// ReSharper restore MemberCanBePrivate.Local
		#endregion stored procedure classes

		#endregion private
	} // class ExperianConsumerCheck
} // namespace
