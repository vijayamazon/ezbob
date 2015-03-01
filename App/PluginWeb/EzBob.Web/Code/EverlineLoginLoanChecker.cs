namespace EzBob.Web.Code {
	using System;
	using ConfigManager;
	using log4net;
	using Newtonsoft.Json;
	using RestSharp;

	public class EverlineLoginLoanChecker {
		private readonly ILog Log = LogManager.GetLogger(typeof (EverlineLoginLoanChecker));
		public EverlineLoginLoanCheckerResult GetLoginStatus(string email) {
			try {
				string testMode = CurrentValues.Instance.EverlineLoanStatusTestMode.Value;
				if (!string.IsNullOrEmpty(testMode)) {
					Log.InfoFormat("EverlineLoginLoanChecker TestMode: {0}", testMode);	
					EverlineLoanStatus status;
					if (Enum.TryParse(CurrentValues.Instance.EverlineLoanStatusTestMode.Value, out status)) {
						Log.InfoFormat("EverlineLoginLoanChecker returning status: {0}", status);
						return new EverlineLoginLoanCheckerResult {
							status = status
						};
					} else {
						Log.WarnFormat("EverlineLoginLoanChecker failed to parse test mode status: {0}", testMode);
					}
				}
				Log.InfoFormat("EverlineLoginLoanChecker executing api call to wonga for email: {0}", email);
				RestClient client = new RestClient("https://restapi.everline.com/1.0/ezbob/customerloanstatus");
				var request = new RestRequest(Method.GET) {
					Parameters = {
						new Parameter {
							Name = "email",
							Type = ParameterType.QueryString,
							Value = email
						}
					},
				};
				request.AddHeader("X-Authentication", "c83c4951-d205-4452-bce0-f0bc24d3e8b2,00000000-0000-0000-0000-000000000000,5a4690c72b1723a54530700250d90e5b35aa283ffb884525a031c3dc0a164c5c");
				var response = client.Execute(request);
				if (string.IsNullOrEmpty(response.Content)) {
					Log.WarnFormat("EverlineLoginLoanChecker empty response returning error {0}", response.ErrorMessage);
					return new EverlineLoginLoanCheckerResult {
						Message = response.ErrorMessage,
						status = EverlineLoanStatus.Error
					};
				}
				var result = JsonConvert.DeserializeObject<EverlineLoginLoanCheckerResult>(response.Content);
				Log.InfoFormat("EverlineLoginLoanChecker successful response {0} returning {1}", response.Content, result.status);
				return result;
			} catch (Exception ex) {
				Log.WarnFormat("EverlineLoginLoanChecker exception during retrieve of status {0}", ex);
				return new EverlineLoginLoanCheckerResult {
					Message = ex.Message,
					status = EverlineLoanStatus.Error
				};
			}
		}
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

	/*[TestFixture]
	public class test {
		[Test]
		public void testLogin() {
			EverlineLoginLoanChecker c = new EverlineLoginLoanChecker();

			var respose = c.GetLoginStatus("locksheathfruiterers@googlemail.com"); //ExistsWithCurrentLiveLoan
			Console.WriteLine(respose.status);
			Assert.AreEqual(EverlineLoanStatus.ExistsWithCurrentLiveLoan, respose.status);
			respose = c.GetLoginStatus("nick.brown@findtheengineer.com"); //ExistsWithNoLiveLoan
			Console.WriteLine(respose.status);
			Assert.AreEqual(EverlineLoanStatus.ExistsWithNoLiveLoan, respose.status);
			respose = c.GetLoginStatus("test@test.com"); //DoesNotExist
			Console.WriteLine(respose.status);
			Assert.AreEqual(EverlineLoanStatus.DoesNotExist, respose.status);
			
		}
	}*/
}