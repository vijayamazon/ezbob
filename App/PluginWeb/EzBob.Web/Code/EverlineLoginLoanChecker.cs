namespace EzBob.Web.Code {
	using System;
	using Newtonsoft.Json;
	using RestSharp;

	public class EverlineLoginLoanChecker {
		public EverlineLoginLoanCheckerResult GetLoginStatus(string email) {
			try {
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