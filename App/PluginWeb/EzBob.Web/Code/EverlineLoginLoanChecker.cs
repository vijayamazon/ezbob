namespace EzBob.Web.Code {
	using System;
	using System.Collections.Generic;
	using ConfigManager;
	using log4net;
	using Newtonsoft.Json;
	using RestSharp;

	public class EverlineLoginLoanChecker {
		protected ILog Log = LogManager.GetLogger(typeof (EverlineLoginLoanChecker));
		private const string EverlineApiBaseUrl = "https://restapi.everline.com/1.0/ezbob/";
		private const string Authentication = "c83c4951-d205-4452-bce0-f0bc24d3e8b2,00000000-0000-0000-0000-000000000000,5a4690c72b1723a54530700250d90e5b35aa283ffb884525a031c3dc0a164c5c";

		public EverlineLoginLoanCheckerResult GetLoginStatus(string email) {
			try {
				if (!string.IsNullOrEmpty(CurrentValues.Instance.EverlineLoanStatusTestMode.Value)) {
					EverlineLoanStatus status;
					if (Enum.TryParse(CurrentValues.Instance.EverlineLoanStatusTestMode.Value, out status)) {
						return new EverlineLoginLoanCheckerResult {
							status = status
						};
					}
				}

				RestClient client = new RestClient(EverlineApiBaseUrl);
				var request = new RestRequest("customerloanstatus", Method.GET) {
					Parameters = {
						new Parameter {
							Name = "email",
							Type = ParameterType.QueryString,
							Value = email
						}
					},
				};
				request.AddHeader("X-Authentication", Authentication);
				var response = client.Execute(request);
				if (string.IsNullOrEmpty(response.Content)) {
					return new EverlineLoginLoanCheckerResult {
						Message = response.ErrorMessage,
						status = EverlineLoanStatus.Error
					};
				}
				var result = JsonConvert.DeserializeObject<EverlineLoginLoanCheckerResult>(response.Content);
				return result;
			} catch (Exception ex) {
				return new EverlineLoginLoanCheckerResult {
					Message = ex.Message,
					status = EverlineLoanStatus.Error
				};
			}
		}

		public EverlineBusinessOrganisationDetails GetLoanDetails(string email) {
			try {
				if(!string.IsNullOrEmpty(CurrentValues.Instance.EverlineLoanStatusTestMode.Value)){
					EverlineLoanStatus status;
					Enum.TryParse(CurrentValues.Instance.EverlineLoanStatusTestMode.Value, out status);
						if(status == EverlineLoanStatus.ExistsWithCurrentLiveLoan){
							var testResult = JsonConvert.DeserializeObject<EverlineBusinessOrganisationDetails>(testResponse);
							return testResult;
						}
				}

				RestClient client = new RestClient(EverlineApiBaseUrl);
				var request = new RestRequest("businessorganisationdetails", Method.GET) {
					Parameters = {
						new Parameter {
							Name = "email",
							Type = ParameterType.QueryString,
							Value = email
						}
					},
				};
				request.AddHeader("X-Authentication", Authentication);
				var response = client.Execute(request);
				var result = JsonConvert.DeserializeObject<EverlineBusinessOrganisationDetails>(response.Content);
				return result;
			} catch (Exception ex) {
				Log.ErrorFormat("Failed to retrieve everline loan details for customer email {0}, exception:\n {1}", email, ex);
				return new EverlineBusinessOrganisationDetails();
			}
		}


		private const string testResponse = @"{""loans"":[{""loanId"":""51273ac4-f38c-438d-9847-50ad0a000023"",""loanAmount"":3200.00,""term"":5,""fundedOn"":""2013-02-22T12:15:18"",""closedOn"":""2013-04-05T06:00:42"",""daysClosedLateCount"":6,""defaultChargesCount"":6,""gaugeScore"":null,""augurScore"":null,""balanceDetails"":{""totalOutstandingBalance"":-10.00},""transactions"":[{""transactionId"":""9fe8d6ff-4b25-4d9c-a2ca-185a9f0dd94e"",""postedOn"":""2013-02-22T12:15:19"",""amount"":3200.00,""reference"":""Fixed installment loan initial advance"",""createdOn"":""2013-02-22T12:15:19"",""transationType"":1},{""transactionId"":""fed26b06-d9b5-43c8-ae40-e00d0809f82d"",""postedOn"":""2013-02-22T12:15:19"",""amount"":96.00,""reference"":""Fixed installment loan application fee"",""createdOn"":""2013-02-22T12:15:19"",""transationType"":2},{""transactionId"":""e3ba7bbd-8488-419b-81e5-f035ba578ac8"",""postedOn"":""2013-02-22T12:15:19"",""amount"":80.00,""reference"":""Fixed installment loan interest"",""createdOn"":""2013-02-22T12:15:20"",""transationType"":4},{""transactionId"":""9dfa3754-94fd-4e6d-a8ff-fad4da89588b"",""postedOn"":""2013-03-01T17:00:57"",""amount"":10.00,""reference"":null,""createdOn"":""2013-03-01T17:00:58"",""transationType"":5},{""transactionId"":""05989f1b-e08f-4644-bfd8-deb958e756e5"",""postedOn"":""2013-03-03T17:01:07"",""amount"":10.00,""reference"":null,""createdOn"":""2013-03-03T17:01:07"",""transationType"":5},{""transactionId"":""d1500423-c50c-4a3b-998a-2f28c83ad12f"",""postedOn"":""2013-03-04T10:29:50"",""amount"":-675.20,""reference"":""Payment card repayment from CS"",""createdOn"":""2013-03-04T10:29:49"",""transationType"":6},{""transactionId"":""beb4867e-2536-483f-a1de-efc9dd2d7d4d"",""postedOn"":""2013-02-28T22:00:00"",""amount"":-10.00,""reference"":""gogw funds were available"",""createdOn"":""2013-03-04T10:32:41"",""transationType"":6},{""transactionId"":""0687ff89-0306-40dd-83b1-d8c5d4c86342"",""postedOn"":""2013-03-02T22:00:00"",""amount"":-10.00,""reference"":""gogw funds were available"",""createdOn"":""2013-03-04T10:33:23"",""transationType"":6},{""transactionId"":""1eb2c2ed-4c68-4bda-8681-14babad6b6fc"",""postedOn"":""2013-03-08T17:00:10"",""amount"":-675.20,""reference"":null,""createdOn"":""2013-03-08T17:00:11"",""transationType"":6},{""transactionId"":""6f01ebb8-a5f4-4a90-9a58-eab42cae9b2a"",""postedOn"":""2013-03-08T17:00:12"",""amount"":-10.00,""reference"":null,""createdOn"":""2013-03-08T17:00:13"",""transationType"":6},{""transactionId"":""5dd6c00e-f6bb-44b1-b113-6661f43049ff"",""postedOn"":""2013-03-15T17:00:20"",""amount"":10.00,""reference"":null,""createdOn"":""2013-03-15T17:00:19"",""transationType"":5},{""transactionId"":""5bf3bcb5-72fe-41ff-b7ac-c6657a814b5f"",""postedOn"":""2013-03-17T17:00:25"",""amount"":10.00,""reference"":null,""createdOn"":""2013-03-17T17:00:26"",""transationType"":5},{""transactionId"":""5fde415d-aaac-43ad-a931-1deb32666268"",""postedOn"":""2013-03-22T17:01:08"",""amount"":-675.20,""reference"":null,""createdOn"":""2013-03-22T17:01:07"",""transationType"":6},{""transactionId"":""896287d7-0677-4b58-b5a7-0e34312070c7"",""postedOn"":""2013-03-29T17:01:11"",""amount"":10.00,""reference"":null,""createdOn"":""2013-03-29T17:01:09"",""transationType"":5},{""transactionId"":""d0ea1206-5ba6-4a8c-ba58-9cfc8e7ee2e0"",""postedOn"":""2013-03-31T18:01:19"",""amount"":10.00,""reference"":null,""createdOn"":""2013-03-31T17:01:18"",""transationType"":5},{""transactionId"":""c93d26fb-599a-4d5d-87eb-1514e9622ec6"",""postedOn"":""2013-04-05T06:00:22"",""amount"":-1350.40,""reference"":null,""createdOn"":""2013-04-05T05:00:26"",""transationType"":6},{""transactionId"":""42f0b362-71f5-46fe-a4ea-8e8a8a0c06f3"",""postedOn"":""2013-04-05T06:00:42"",""amount"":-40.00,""reference"":null,""createdOn"":""2013-04-05T05:00:44"",""transationType"":6},{""transactionId"":""b242713a-5b1d-4431-a4f3-5351232d0fb5"",""postedOn"":""2013-07-22T17:45:00"",""amount"":10.00,""reference"":""Refund"",""createdOn"":""2013-07-22T16:45:01"",""transationType"":15}]},{""loanId"":""543faaa3-06fc-4333-b38d-0bc50a000023"",""loanAmount"":3000.00,""term"":10,""fundedOn"":""2014-10-16T15:08:35"",""closedOn"":""2014-12-24T09:49:31"",""daysClosedLateCount"":-1,""defaultChargesCount"":1,""gaugeScore"":516,""augurScore"":386,""balanceDetails"":{""totalOutstandingBalance"":0.00},""transactions"":[{""transactionId"":""de7d1f4b-0efc-46fe-9007-39440888b38e"",""postedOn"":""2014-10-16T16:08:35"",""amount"":3000.00,""reference"":""Fixed installment loan initial advance"",""createdOn"":""2014-10-16T15:08:34"",""transationType"":1},{""transactionId"":""b2744fda-3495-4e5d-a7a6-c45b3fcc0459"",""postedOn"":""2014-10-16T16:08:35"",""amount"":45.00,""reference"":""Fixed installment loan application fee"",""createdOn"":""2014-10-16T15:08:35"",""transationType"":2},{""transactionId"":""5adcb4e2-4883-4dff-a16c-90425dc25915"",""postedOn"":""2014-10-16T16:08:35"",""amount"":150.00,""reference"":""Fixed installment loan interest"",""createdOn"":""2014-10-16T15:08:35"",""transationType"":4},{""transactionId"":""c0f60362-33d5-480c-8d99-06e117bed41c"",""postedOn"":""2014-10-23T17:03:16"",""amount"":-319.50,""reference"":null,""createdOn"":""2014-10-23T16:03:16"",""transationType"":6},{""transactionId"":""73f242ee-08de-4fff-ad09-be4a3ab8b22f"",""postedOn"":""2014-10-30T05:03:18"",""amount"":-319.50,""reference"":null,""createdOn"":""2014-10-30T05:03:33"",""transationType"":6},{""transactionId"":""2b52c0c1-86e5-46b4-baf3-87de7e705def"",""postedOn"":""2014-11-06T05:02:54"",""amount"":-319.50,""reference"":null,""createdOn"":""2014-11-06T05:03:08"",""transationType"":6},{""transactionId"":""31cce8be-f4b6-4b5b-9025-3a026663f8ca"",""postedOn"":""2014-11-13T17:03:07"",""amount"":5.00,""reference"":null,""createdOn"":""2014-11-13T17:03:08"",""transationType"":5},{""transactionId"":""84fc301a-db4b-450c-8124-c97240282d13"",""postedOn"":""2014-11-13T17:03:09"",""amount"":-5.00,""reference"":null,""createdOn"":""2014-11-13T17:03:11"",""transationType"":6},{""transactionId"":""d168ee29-f3e0-4f26-a630-6ed159d3c4fb"",""postedOn"":""2014-11-14T16:23:46"",""amount"":-319.50,""reference"":""Payment card repayment from CS"",""createdOn"":""2014-11-14T16:23:46"",""transationType"":6},{""transactionId"":""903b9078-1a8e-47e6-a574-a8ac6ee58d35"",""postedOn"":""2014-11-14T16:24:36"",""amount"":-5.00,""reference"":""GOGW"",""createdOn"":""2014-11-14T16:24:36"",""transationType"":30},{""transactionId"":""92886057-c4f1-49e8-8fd2-d7d86ec6a560"",""postedOn"":""2014-11-20T05:02:35"",""amount"":-319.50,""reference"":null,""createdOn"":""2014-11-20T05:02:48"",""transationType"":6},{""transactionId"":""4bc46b37-4278-455f-9876-10ce90814794"",""postedOn"":""2014-11-27T05:02:24"",""amount"":-319.50,""reference"":null,""createdOn"":""2014-11-27T05:02:37"",""transationType"":6},{""transactionId"":""1e494e8c-c515-4099-bf88-f4ea3a41e9dc"",""postedOn"":""2014-12-04T17:02:52"",""amount"":-319.50,""reference"":null,""createdOn"":""2014-12-04T17:02:52"",""transationType"":6},{""transactionId"":""03e13ea9-75f6-4aca-a0f7-ac0fb53e8ca0"",""postedOn"":""2014-12-11T17:02:47"",""amount"":-319.50,""reference"":null,""createdOn"":""2014-12-11T17:02:46"",""transationType"":6},{""transactionId"":""ea5ab58f-5939-4139-bb2b-ad1fbc5205b3"",""postedOn"":""2014-12-18T17:02:24"",""amount"":-319.50,""reference"":null,""createdOn"":""2014-12-18T17:02:22"",""transationType"":6},{""transactionId"":""3c8c9f89-cbc6-4354-8457-4ad0ecb30d2e"",""postedOn"":""2014-12-24T09:49:31"",""amount"":-314.50,""reference"":""Payment card repayment from CS"",""createdOn"":""2014-12-24T09:49:29"",""transationType"":6}]},{""loanId"":""549fe779-4e5c-4104-aaff-13780a000023"",""loanAmount"":3000.00,""term"":3,""fundedOn"":""2014-12-30T17:54:50"",""closedOn"":""2015-01-22T14:39:02"",""daysClosedLateCount"":1,""defaultChargesCount"":2,""gaugeScore"":398,""augurScore"":386,""balanceDetails"":{""totalOutstandingBalance"":0.00},""transactions"":[{""transactionId"":""0f060e7c-2513-47ed-b18d-924e811fc9ef"",""postedOn"":""2014-12-30T17:54:50"",""amount"":3000.00,""reference"":""Fixed installment loan initial advance"",""createdOn"":""2014-12-30T17:54:51"",""transationType"":1},{""transactionId"":""fef5b183-cc3f-4988-bcba-0ee379328907"",""postedOn"":""2014-12-30T17:54:50"",""amount"":45.00,""reference"":""Fixed installment loan application fee"",""createdOn"":""2014-12-30T17:54:51"",""transationType"":2},{""transactionId"":""bec052b3-402d-464c-be91-e7b9cea2c9fa"",""postedOn"":""2014-12-30T17:54:50"",""amount"":45.00,""reference"":""Fixed installment loan interest"",""createdOn"":""2014-12-30T17:54:51"",""transationType"":4},{""transactionId"":""aa6d18a5-67c2-47a0-b218-1d2007fd1ca2"",""postedOn"":""2015-01-06T17:03:01"",""amount"":-1030.00,""reference"":null,""createdOn"":""2015-01-06T17:03:01"",""transationType"":6},{""transactionId"":""de70120d-7ce4-4d05-908d-644d45173da0"",""postedOn"":""2015-01-13T17:03:13"",""amount"":5.00,""reference"":null,""createdOn"":""2015-01-13T17:03:13"",""transationType"":5},{""transactionId"":""1ece32eb-6b1e-4af3-8ab9-e42be4782fc5"",""postedOn"":""2015-01-13T17:03:16"",""amount"":-5.00,""reference"":null,""createdOn"":""2015-01-13T17:03:15"",""transationType"":6},{""transactionId"":""f9d7e16b-345b-4076-9b95-45b75e5ff5ce"",""postedOn"":""2015-01-14T17:01:05"",""amount"":-1025.00,""reference"":""Payment card repayment from CS"",""createdOn"":""2015-01-14T17:01:05"",""transationType"":6},{""transactionId"":""8fb92301-cf1a-40c3-b3e1-8ccfc7384cc3"",""postedOn"":""2015-01-14T17:06:05"",""amount"":-5.00,""reference"":""GOGW"",""createdOn"":""2015-01-14T17:09:55"",""transationType"":5},{""transactionId"":""c0af444b-0857-4845-80f2-6c3831e8c8a8"",""postedOn"":""2015-01-20T17:03:19"",""amount"":5.00,""reference"":null,""createdOn"":""2015-01-20T17:03:18"",""transationType"":5},{""transactionId"":""9d5c64aa-74b4-4b45-b555-899d9ff5aa55"",""postedOn"":""2015-01-20T17:03:21"",""amount"":-5.00,""reference"":null,""createdOn"":""2015-01-20T17:03:19"",""transationType"":6},{""transactionId"":""0b24344b-d7b0-46a1-b6e2-815758e49574"",""postedOn"":""2015-01-21T16:47:56"",""amount"":-950.00,""reference"":""Payment card repayment from CS"",""createdOn"":""2015-01-21T16:47:55"",""transationType"":6},{""transactionId"":""d87baf84-2f78-4526-890d-5c089ce6692e"",""postedOn"":""2015-01-22T14:38:21"",""amount"":-70.00,""reference"":""Payment card repayment from CS"",""createdOn"":""2015-01-22T14:38:23"",""transationType"":6},{""transactionId"":""49b1a8fa-df55-4f14-abad-7f2dbd61f826"",""postedOn"":""2015-01-22T14:39:02"",""amount"":-10.00,""reference"":""GOGW"",""createdOn"":""2015-01-22T14:39:03"",""transationType"":5}]},{""loanId"":""54d87cb3-ba48-4bca-b3d2-30b80a000024"",""loanAmount"":3000.00,""term"":10,""fundedOn"":""2015-02-09T11:50:26"",""closedOn"":null,""daysClosedLateCount"":null,""defaultChargesCount"":0,""gaugeScore"":447,""augurScore"":377,""balanceDetails"":{""totalOutstandingBalance"":1917.00},""transactions"":[{""transactionId"":""172e291f-421e-4f32-8619-0c1148e8ad25"",""postedOn"":""2015-02-09T11:50:26"",""amount"":3000.00,""reference"":""Fixed installment loan initial advance"",""createdOn"":""2015-02-09T11:50:26"",""transationType"":1},{""transactionId"":""366253a5-ecf7-4522-9593-213c104c13fb"",""postedOn"":""2015-02-09T11:50:26"",""amount"":45.00,""reference"":""Fixed installment loan application fee"",""createdOn"":""2015-02-09T11:50:26"",""transationType"":2},{""transactionId"":""737d7e7b-2650-402b-8dcc-f1a22d4a9010"",""postedOn"":""2015-02-09T11:50:26"",""amount"":150.00,""reference"":""Fixed installment loan interest"",""createdOn"":""2015-02-09T11:50:27"",""transationType"":4},{""transactionId"":""91f3908b-c70a-4da0-b83e-d3eb6ca1ad02"",""postedOn"":""2015-02-16T05:02:39"",""amount"":-319.50,""reference"":null,""createdOn"":""2015-02-16T05:02:44"",""transationType"":6},{""transactionId"":""8663be33-ac40-4ff4-9693-d2f035156dbc"",""postedOn"":""2015-02-23T17:03:15"",""amount"":-319.50,""reference"":null,""createdOn"":""2015-02-23T17:03:14"",""transationType"":6},{""transactionId"":""d9869825-28df-4acf-b14a-1f84997d755a"",""postedOn"":""2015-03-02T17:02:32"",""amount"":-319.50,""reference"":null,""createdOn"":""2015-03-02T17:02:32"",""transationType"":6},{""transactionId"":""805c0be2-e1f1-43a5-9ecc-25cfd2ab5a3f"",""postedOn"":""2015-03-09T05:02:39"",""amount"":-319.50,""reference"":null,""createdOn"":""2015-03-09T05:02:44"",""transationType"":6}]}],""loansCount"":4,""firstLoanFundedDate"":""2013-02-22T12:15:18""}";
	}

	public enum EverlineLoanStatus {
		Error,
		DoesNotExist,
		ExistsWithNoLiveLoan,
		ExistsWithCurrentLiveLoan
	}

	public class EverlineLoginLoanCheckerResult {
		public EverlineLoanStatus status { get; set; }
		public string Message { get; set; }
	}

	public class EverlineBusinessOrganisationDetails {
		[JsonProperty("loans")]
		public IEnumerable<EverlineLoanApplication> LoanApplications { get; set; }

		[JsonProperty("loansCount")]
		public int LoanApplicationsCount { get; set; }

		[JsonProperty("firstLoanFundedDate")]
		public DateTime? FirstLoanApplicationFundedDate { get; set; }
	}

	public class EverlineLoanApplication {
		[JsonProperty("loanId")]
		public Guid LoanId { get; set; }

		[JsonProperty("loanAmount")]
		public decimal LoanAmount { get; set; }

		[JsonProperty("term")]
		public int Term { get; set; }

		[JsonProperty("fundedOn")]
		public DateTime? FundedOn { get; set; }

		[JsonProperty("closedOn")]
		public DateTime? ClosedOn { get; set; }

		[JsonProperty("daysClosedLateCount")]
		public int? DaysClosedLateCount { get; set; }

		[JsonProperty("defaultChargesCount")]
		public int? DefaultChargesCount { get; set; }

		[JsonProperty("gaugeScore")]
		public int? GaugeScore { get; set; }

		[JsonProperty("augurScore")]
		public int? AugurScore { get; set; }

		[JsonProperty("balanceDetails")]
		public EverlineBalanceDetails BalanceDetails { get; set; }

		[JsonProperty("transactions")]
		public IEnumerable<EverlineLoanApplicationTransaction> Transactions { get; set; }
	}

	public class EverlineLoanApplicationTransaction {
		[JsonProperty("transactionId")]
		public Guid TransactionId { get; set; }

		[JsonProperty("postedOn")]
		public DateTime? PostedOn { get; set; }

		[JsonProperty("amount")]
		public decimal Amount { get; set; }

		[JsonProperty("reference")]
		public string Reference { get; set; }

		[JsonProperty("createdOn")]
		public DateTime? CreatedOn { get; set; }

		[JsonProperty("transationType")]
		public EverlineTransactionType TransactionType { get; set; }
	}

	public class EverlineBalanceDetails {
		[JsonProperty("totalOutstandingBalance")]
		public decimal? TotalOutstandingBalance { get; set; }
	}

	public enum EverlineTransactionType {
		CashAdvance = 1,
		Fee = 2,
		LoanExtensionFee = 3,
		Interest = 4,
		DefaultCharge = 5,
		CardPayment = 6,
		WriteOff = 7,
		DebtSale = 8,
		DebtSaleProceeds = 9,
		InterestWriteOff = 10,
		AgencyFee = 11,
		Adjustment = 12,
		ArrearsLetterCharge = 13,
		OutsourcedAgencyPayment = 14,
		Refund = 15,
		Chargeback = 16,
		Cheque = 17,
		DirectBankPayment = 18,
		SuspendInterestAccrual = 19,
		ResumeInterestAccrual = 20,
		V2MigratedInsurance = 21,
		V2MigratedWriteOff = 22,
		V2MigratedCharge = 23,
		V2MigratedPayment = 24,
		V2Migratedadvance = 25,
		ServiceFee = 26,
		InitiationFee = 27,
		InterestRate = 28,
		InterestRateFee = 29,
		InterestAdjustment = 30,
		RegularCardPayment = 31,
		Dispute = 32,
		DirectDeposit = 33,
		InterestReversal = 34,
		ServiceFeeWriteOff = 35,
		ServiceFeeReversal = 36,
		InitiationFeeWriteOff = 37,
		InitiationFeeReversal = 38,
		CashAdvanceReversal = 39,
		FeeReversal = 40,
		BatchedDcaReceipt = 41,
		BatchedDmcReceipt = 42,
		LoanExtensionCardPayment = 43,
		SuspendServiceFee = 44,
		ResumeServiceFee = 45,
		ProductCommissionFee = 46,
		MerchantPaidChargeback = 49,
		MarketingIncentive = 50,
		SuspendDefaultCharge = 51,
		ResumeDefaultCharge = 52,
		ArrearsCommissionFee = 53,
		SuspendTransmissionFee = 54,
		Compensation = 55,
		SettlementOfferWriteOff = 56,
		FraudWriteOff = 57,
		DebtSaleWriteOff = 58,
		LoanExtensionPayment = 59,
	}


	//[TestFixture]
	//public class test {
	//	[SetUp]
	//	public void setup() {
	//		//TODO in order to test to work need to init log, db and configuration variables
	//		var log = new ConsoleLog();
	//		var db = DbConnectionGenerator.Get(log);
	//		CurrentValues.Init(db, log);
	//	}
	//	[Test]
	//	public void testLogin() {
	//		EverlineLoginLoanChecker c = new EverlineLoginLoanChecker();

	//		var respose = c.GetLoginStatus("locksheathfruiterers@googlemail.com"); //ExistsWithCurrentLiveLoan
	//		Console.WriteLine(respose.status);
	//		Assert.AreEqual(EverlineLoanStatus.ExistsWithCurrentLiveLoan, respose.status);
	//		respose = c.GetLoginStatus("nick.brown@findtheengineer.com"); //ExistsWithNoLiveLoan
	//		Console.WriteLine(respose.status);
	//		Assert.AreEqual(EverlineLoanStatus.ExistsWithNoLiveLoan, respose.status);
	//		respose = c.GetLoginStatus("test@test.com"); //DoesNotExist
	//		Console.WriteLine(respose.status);
	//		Assert.AreEqual(EverlineLoanStatus.DoesNotExist, respose.status);

	//	}

	//	[Test]
	//	public void testLoanDetails() {
	//		EverlineLoginLoanChecker c = new EverlineLoginLoanChecker();

	//		var respose = c.GetLoanDetails("locksheathfruiterers@googlemail.com"); //ExistsWithCurrentLiveLoan
	//		Console.WriteLine(JsonConvert.SerializeObject(respose));
	//		Assert.IsNotNull(respose.LoanApplications);
	//		Assert.Greater(0, respose.LoanApplications.Where(x => !x.ClosedOn.HasValue).Sum(x => x.BalanceDetails.TotalOutstandingBalance));
	//	}
	//}

}