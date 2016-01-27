namespace Ezbob.Backend.Strategies.CompaniesHouse {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Net.Http;
	using System.Net.Http.Headers;
	using Ezbob.Backend.ModelsWithDB.CompaniesHouse;
	using Ezbob.Database;
	using Newtonsoft.Json;

	public class GetCompaniesHouseData: AStrategy {
		private readonly int? customerID;
		private readonly string companyRefNum;
		private readonly bool forceCheck;

		public GetCompaniesHouseData(int? customerID, string companyRefNum, bool forceCheck) {
			this.customerID = customerID;
			this.companyRefNum = companyRefNum;
			this.forceCheck = forceCheck;
		}

		public override string Name { get { return "GetCompaniesHouseData"; } }

		public override void Execute() {
			if (string.IsNullOrEmpty(this.companyRefNum) && !this.customerID.HasValue) {
				Log.Error("GetCompaniesHouseData failed, should provide customer id or companyRefNum");
				return;
			}

			string companyRef = this.companyRefNum;
			if (string.IsNullOrEmpty(this.companyRefNum) && this.customerID.HasValue) {
				companyRef = DB.ExecuteScalar<string>(string.Format("SELECT co.ExperianRefNum FROM Customer c LEFT JOIN Company co ON c.CompanyId = co.Id WHERE c.Id = {0}", this.customerID.Value), CommandSpecies.Text);
			}

			if (string.IsNullOrEmpty(companyRef) || companyRef == "NotFound") {
				Log.Error("GetCompaniesHouseData failed to retrieve customer's company ref number {0}", companyRef);
				return;
			}

			if (!string.IsNullOrEmpty(companyRef)) {
				if (this.forceCheck) {
					RetrieveDataFromCompaniesApi(companyRef);
				} else {
					if (!CheckCache(companyRef)) {
						RetrieveDataFromCompaniesApi(companyRef);
					}
				}
			}
		}

		private bool CheckCache(string companyRef) {
			CompaniesHouseOfficerOrder result = new CompaniesHouseOfficerOrder();
			List<CompaniesHouseOfficerAppointmentOrder> appointmentsOrders = new List<CompaniesHouseOfficerAppointmentOrder>();
			List<CompaniesHouseOfficerAppointmentOrderItem> appointmentsOrderItems = new List<CompaniesHouseOfficerAppointmentOrderItem>();
			var data = DB.ExecuteEnumerable(
				"LoadFullCompaniesHouse",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CompanyRefNum", companyRef)
				);

			foreach (SafeReader sr in data) {
				string sType = sr["DatumType"];

				switch (sType) {
					case "CompaniesHouseOfficerOrder":
						result = sr.Fill<CompaniesHouseOfficerOrder>();
						break;

					case "CompaniesHouseOfficerOrderItem":
						var officerOrderItem = sr.Fill<CompaniesHouseOfficerOrderItem>();
						result.Officers.Add(officerOrderItem);
						break;

					case "CompaniesHouseOfficerAppointmentOrder":
						var officerAppointmentOrder = sr.Fill<CompaniesHouseOfficerAppointmentOrder>();
						appointmentsOrders.Add(officerAppointmentOrder);
						break;

					case "CompaniesHouseOfficerAppointmentOrderItem":
						var officerAppointmentOrderItem = sr.Fill<CompaniesHouseOfficerAppointmentOrderItem>();
						appointmentsOrderItems.Add(officerAppointmentOrderItem);
						break;
				} // switch
			} // for each row

			foreach (var appointmentsOrder in appointmentsOrders) {
				var officer = result.Officers.FirstOrDefault(x => x.CompaniesHouseOfficerOrderItemID == appointmentsOrder.CompaniesHouseOfficerOrderItemID);
				if (officer != null) {
					officer.AppointmentOrder = appointmentsOrder;
				}
			}

			foreach (var appointmentsOrderItem in appointmentsOrderItems) {
				var officer = result.Officers.FirstOrDefault(x => x.AppointmentOrder.CompaniesHouseOfficerAppointmentOrderID == appointmentsOrderItem.CompaniesHouseOfficerAppointmentOrderID);
				if (officer != null ) {
					if (officer.AppointmentOrder.Appointments == null) {
						officer.AppointmentOrder.Appointments = new List<CompaniesHouseOfficerAppointmentOrderItem>();
					}
					officer.AppointmentOrder.Appointments.Add(appointmentsOrderItem);
				}
			}

			Result = result;
			return result.CompaniesHouseOfficerOrderID != 0;
		}//CheckCache

		private void RetrieveDataFromCompaniesApi(string companyRef) {
			var model = new CompaniesHouseOfficerOrder {
				CompanyRefNum = companyRef,
				Timestamp = DateTime.UtcNow,
				Officers = new List<CompaniesHouseOfficerOrderItem>()
			};
			try {
				HttpClient httpClient = new HttpClient();
				httpClient.BaseAddress = new Uri("https://api.companieshouse.gov.uk/");
				httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(ConfigManager.CurrentValues.Instance.CompaniesHouseApiKey)));

				const int itemsPerPage = 35; //default
				int startIndexOfficers = 0;
				bool shouldRetrieveOfficers = true;
				do {
					//Get officers of company by ref num
					var result = httpClient.GetAsync(string.Format("company/{0}/officers/?items_per_page={1}&start_index={2}", companyRef, itemsPerPage, startIndexOfficers))
						.Result.Content.ReadAsStringAsync()
						.Result;
					var officersList = JsonConvert.DeserializeObject<OfficersListResult>(result);

					if (officersList != null) {
						officersList.Fill(model);
						foreach (var officer in officersList.items) {
							var officerModel = new CompaniesHouseOfficerOrderItem();
							officer.Fill(officerModel);
							if (officer.links != null && officer.links.officer != null && !string.IsNullOrEmpty(officer.links.officer.appointments)) {
								int startIndexAppointments = 0;
								bool shouldRetrieveAppointments = true;
								do {
									//Get appointments by officer ref num
									var appointmentsResult = httpClient.GetAsync(string.Format("{0}/?items_per_page={1}&start_index={2}", officer.links.officer.appointments, itemsPerPage, startIndexAppointments))
										.Result.Content.ReadAsStringAsync()
										.Result;
									var appointment = JsonConvert.DeserializeObject<AppointmentListResult>(appointmentsResult);
									if (appointment != null) {
										appointment.Fill(officerModel.AppointmentOrder);
									}
									if (appointment != null && appointment.total_results > (startIndexAppointments + itemsPerPage)) {
										startIndexAppointments += itemsPerPage;
									} else {
										shouldRetrieveAppointments = false;
									}
								} while (shouldRetrieveAppointments);
							} //if
							model.Officers.Add(officerModel);
						} //foreach
					} //if

					if (officersList != null && officersList.total_results > (startIndexOfficers + itemsPerPage)) {
						startIndexOfficers += itemsPerPage;
					} else {
						shouldRetrieveOfficers = false;
					}
				} while (shouldRetrieveOfficers);
			} catch (Exception ex) {
				Log.Error(ex, "Failed to retrieve data from companies house for company {0}", companyRef);
			}
			
			SaveToDB(model);
			Result = model;
		}//RetrieveDataFromCompaniesData

		private void SaveToDB(CompaniesHouseOfficerOrder model) {
			try {
			int companiesHouseOfficerOrderID = DB.ExecuteScalar<int>("CompaniesHouseOfficerOrderSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("@Tbl", model));
			foreach (var officer in model.Officers) {
				officer.CompaniesHouseOfficerOrderID = companiesHouseOfficerOrderID;
				int companiesHouseOfficerOrderItemID = DB.ExecuteScalar<int>("CompaniesHouseOfficerOrderItemSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("@Tbl", officer));
				officer.CompaniesHouseOfficerOrderItemID = companiesHouseOfficerOrderItemID;
				officer.AppointmentOrder.CompaniesHouseOfficerOrderItemID = companiesHouseOfficerOrderItemID;
				int companiesHouseOfficerAppointmentOrderID = DB.ExecuteScalar<int>("CompaniesHouseOfficerAppointmentOrderSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("@Tbl", officer.AppointmentOrder));
				foreach (var appointment in officer.AppointmentOrder.Appointments) {
					appointment.CompaniesHouseOfficerAppointmentOrderID = companiesHouseOfficerAppointmentOrderID;
					int companiesHouseOfficerAppointmentOrderItemID = DB.ExecuteScalar<int>("CompaniesHouseOfficerAppointmentOrderItemSave", CommandSpecies.StoredProcedure, DB.CreateTableParameter("@Tbl", appointment));
					Log.Debug("companiesHouseOfficerAppointmentOrderItemID {0}", companiesHouseOfficerAppointmentOrderItemID);
				}//foreach
			}//foreach
			} catch (Exception ex) {
				Log.Error(ex, "Failed to save companies house data to db for company {0}", model.CompanyRefNum);
			}
		}//SaveToDB

		public CompaniesHouseOfficerOrder Result { get; private set; }
	}//GetCompaniesHouseData
}//ns
