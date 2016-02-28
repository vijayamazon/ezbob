namespace Ezbob.Backend.Strategies.Experian {
	using System;
	using System.Globalization;
	using ExperianLib;
	using EzBobIntegration.Web_References.Consumer;
	using Ezbob.Backend.ModelsWithDB.Experian;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Backend.Strategies.StoredProcs;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class ExperianConsumerCheck : AStrategy {
		public ExperianConsumerCheck(int customerId, int? directorId, bool bForceCheck) {
			this.doSilentAutomation = true;

			this.customerId = customerId;
			this.directorId = directorId.HasValue && directorId.Value == 0 ? null : directorId;
			this.forceCheck = bForceCheck;

			this.personalData =
				new GetPersonalInfoForConsumerCheck(customerId, directorId, DB, Log)
					.FillFirst<GetPersonalInfoForConsumerCheck.ResultRow>();

			this.addressLines = new GetCustomerAddresses(customerId, directorId, DB, Log)
				.FillFirst<GetCustomerAddresses.ResultRow>();
		} // constructor

		public ExperianConsumerCheck PreventSilentAutomation() {
			this.doSilentAutomation = false;
			return this;
		} // PreventSilentAutomation

		public int Score { get; private set; }
		public ExperianConsumerData Result { get; private set; }

		public override string Name {
			get { return "Experian consumer check"; }
		} // Name

		public override void Execute() {
			Log.Info(
				"Starting consumer check with current address, parameters: {0} {1}.",
				this.personalData,
				this.addressLines
			);

			bool bSuccess = GetConsumerInfoAndSave(AddressCurrency.Current);

			if (!bSuccess && CanUsePrevAddress()) {
				Log.Info(
					"Starting consumer check with previous address parameters: {0} {1}.",
					this.personalData,
					this.addressLines
				);
				bSuccess = GetConsumerInfoAndSave(AddressCurrency.Previous);
			} // if

			Log.Say(
				bSuccess ? Severity.Info : Severity.Alert,
				"Consumer check {2} with parameters: {0} {1}.",
				this.personalData,
				this.addressLines,
				bSuccess ? "succeeded" : "failed"
			);

			if (this.doSilentAutomation)
				new SilentAutomation(this.customerId).SetTag(SilentAutomation.Callers.Consumer).Execute();
		} // Execute

		private bool CanUsePrevAddress() {
			return
				(this.directorId == null) &&
				(this.personalData.TimeAtAddress == 1) &&
				!string.IsNullOrEmpty(this.addressLines[6, AddressCurrency.Previous]);
		} // CanUsePrevAddress

		private bool GetConsumerInfoAndSave(AddressCurrency oAddressCurrency) {
			InputLocationDetailsMultiLineLocation location = this.addressLines.GetLocation(oAddressCurrency);

			var ukAddress = new Models.CustomerAddressModel {
				Line1 = location.LocationLine1,
				Line2 = location.LocationLine2,
				Line3 = location.LocationLine3,
				PostCode = location.LocationLine6,
				City = location.LocationLine4
			};
			ukAddress.FillDetails();

			InputLocationDetailsUKLocation ukLokation = new InputLocationDetailsUKLocation {
				Postcode = ukAddress.PostCode,
				HouseName = ukAddress.HouseName,
				HouseNumber = ukAddress.HouseNumber,
				Flat = ukAddress.FlatOrApartmentNumber,
				PostTown = ukAddress.City,
				Street = ukAddress.Address1,
				Street2 = ukAddress.Address2,
				POBox = ukAddress.POBox
			};
			var consumerService = new ConsumerService();

			Result = consumerService.GetConsumerInfo(
				this.personalData.FirstName,
				this.personalData.Surname,
				this.personalData.Gender,
				this.personalData.DateOfBirth,
				ukLokation,
				location,
				"PL", this.customerId, this.directorId,
				false, this.directorId != null, this.forceCheck
			);

			if (Result != null && !Result.HasExperianError)
				Score = Result.BureauScore.HasValue ? Result.BureauScore.Value : 0;

			return Result == null || !Result.HasExperianError;
		} // GetConsumerInfoAndSave

		// ReSharper disable MemberCanBePrivate.Local
		// ReSharper disable UnusedAutoPropertyAccessor.Local
		private class GetPersonalInfoForConsumerCheck : AStoredProcedure {
			public GetPersonalInfoForConsumerCheck(
				int customerId,
				int? directorId,
				AConnection db,
				ASafeLog log
			) : base(db, log) {
				CustomerId = customerId;
				DirectorId = directorId;
			} // constructor

			public override bool HasValidParameters() {
				return CustomerId > 0;
			} // HasValidParameters

			public int CustomerId { get; set; }

			public int? DirectorId { get; set; }

			public class ResultRow : AResultRow {
				public string FirstName { get; set; }
				public string Surname { get; set; }
				public string Gender { get; set; }
				public DateTime DateOfBirth { get; set; }
				public int TimeAtAddress { get; set; }

				public override string ToString() {
					return string.Format(
						"FirstName='{0}' Surname='{1}' Gender='{2}' DateOfBirth='{3}' Time at address={4}",
						FirstName,
						Surname,
						Gender,
						DateOfBirth.ToString("d/MMM/yyyy", CultureInfo.InvariantCulture),
						TimeAtAddress
					);
				} // ToString
			} // class ResultRow
		} // class GetPersonalInfoForConsumerCheck

		private readonly int customerId;
		private readonly int? directorId;
		private readonly bool forceCheck;

		private readonly GetCustomerAddresses.ResultRow addressLines;
		private readonly GetPersonalInfoForConsumerCheck.ResultRow personalData;

		private bool doSilentAutomation;
	} // class ExperianConsumerCheck

	internal static class CustomerAddressExt {
		public static InputLocationDetailsMultiLineLocation GetLocation(
			this GetCustomerAddresses.ResultRow oAddr,
			AddressCurrency oCurrency
		) {
			if (oAddr == null)
				throw new ArgumentNullException("oAddr", "Address row not specified.");

			switch (oCurrency) {
			case AddressCurrency.Current:
				return new InputLocationDetailsMultiLineLocation {
					LocationLine1 = oAddr.Line1,
					LocationLine2 = oAddr.Line2,
					LocationLine3 = oAddr.Line3,
					LocationLine4 = oAddr.Line4,
					LocationLine5 = oAddr.Line5,
					LocationLine6 = oAddr.Line6,
				};

			case AddressCurrency.Previous:
				return new InputLocationDetailsMultiLineLocation {
					LocationLine1 = oAddr.Line1Prev,
					LocationLine2 = oAddr.Line2Prev,
					LocationLine3 = oAddr.Line3Prev,
					LocationLine4 = oAddr.Line4Prev,
					LocationLine5 = oAddr.Line5Prev,
					LocationLine6 = oAddr.Line6Prev,
				};

			default:
				throw new ArgumentOutOfRangeException("oCurrency", "Unsupported value: " + oCurrency);
			} // switch
		} // GetLocation
	} // class CustomerAddressExt
} // namespace
