namespace Ezbob.Backend.Strategies {
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Linq;
	using ApplicationMng.Repository;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Misc;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using EzBob.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Loans;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using log4net;
	using LandRegistryLib;
	using NHibernate;
	using StructureMap;

	public class StrategyHelper {
		public StrategyHelper() {
			_session = ObjectFactory.GetInstance<ISession>();
			_customers = ObjectFactory.GetInstance<CustomerRepository>();
			_caisReportsHistoryRepository = ObjectFactory.GetInstance<CaisReportsHistoryRepository>();
			loanRepository = ObjectFactory.GetInstance<LoanRepository>();
			customerAddressRepository = ObjectFactory.GetInstance<CustomerAddressRepository>();
			landRegistryRepository = ObjectFactory.GetInstance<LandRegistryRepository>();
			_customerAnalytics = ObjectFactory.GetInstance<CustomerAnalyticsRepository>();
		}

		public static bool AreEqual(string a, string b) {
			return string.IsNullOrEmpty(a) ? string.IsNullOrEmpty(b) : string.Equals(a, b);
		}

		public string GetCAISFileById(int id) {
			var file = _caisReportsHistoryRepository.Get(id);
			return file != null ? ZipString.Unzip(file.FileData) : "";
		}

		public DateTime GetCustomerIncorporationDate(Customer customer = null) {
			if (customer == null)
				return DateTime.UtcNow;

			bool bIsLimited =
				(customer.Company != null) &&
				(customer.Company.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited);

			if (bIsLimited) {
				CustomerAnalytics oAnalytics = _customerAnalytics.GetAll()
					.FirstOrDefault(ca => ca.Id == customer.Id);

				DateTime oIncorporationDate = (oAnalytics != null) ? oAnalytics.IncorporationDate : DateTime.UtcNow;

				if (oIncorporationDate.Year < 1000)
					oIncorporationDate = DateTime.UtcNow;

				return oIncorporationDate;
			} // if ltd

			System.Data.IDbCommand cmd = _session.Connection.CreateCommand();
			cmd.CommandText = "GetNoLtdIncorporationDate";
			cmd.CommandType = CommandType.StoredProcedure;

			IDbDataParameter prm = cmd.CreateParameter();
			prm.ParameterName = "@CustomerID";
			prm.Direction = ParameterDirection.Input;
			prm.DbType = DbType.Int32;
			prm.Value = customer.Id;
			cmd.Parameters.Add(prm);

			DateTime? oDate = (DateTime?)cmd.ExecuteScalar();

			return oDate ?? DateTime.UtcNow;
		}

		public LandRegistryDataModel GetLandRegistryData(int customerId, string titleNumber, out LandRegistry landRegistry) {
			log.DebugFormat("GetLandRegistryData begin cId {0} titleNumber {1}", customerId, titleNumber);
			var lrRepo = ObjectFactory.GetInstance<LandRegistryRepository>();

			//check cash
			var cache = lrRepo.GetRes(customerId, titleNumber);
			if (cache != null) {
				var b = new LandRegistryModelBuilder();
				var cacheModel = new LandRegistryDataModel {
					Request = cache.Request,
					Response = cache.Response,
					Res = b.BuildResModel(cache.Response),
					RequestType = cache.RequestType,
					ResponseType = cache.ResponseType,
					DataSource = LandRegistryDataSource.Cache
				};

				if (!cache.Owners.Any()) {
					var owners = new List<LandRegistryOwner>();
					foreach (var owner in cacheModel.Res.Proprietorship.ProprietorshipParties) {
						owners.Add(new LandRegistryOwner {
							LandRegistry = cache,
							FirstName = owner.PrivateIndividualForename,
							LastName = owner.PrivateIndividualSurname,
							CompanyName = owner.CompanyName,
							CompanyRegistrationNumber = owner.CompanyRegistrationNumber,
						});
					}
					cache.Owners = owners;

					lrRepo.SaveOrUpdate(cache);
				}

				landRegistry = cache;
				return cacheModel;
			}

			var isProd = CurrentValues.Instance.LandRegistryProd;

			ILandRegistryApi lr;
			if (isProd) {
				lr = new LandRegistryApi(
					CurrentValues.Instance.LandRegistryUserName,
					Encrypted.Decrypt(CurrentValues.Instance.LandRegistryPassword),
					CurrentValues.Instance.LandRegistryFilePath);
			} else
				lr = new LandRegistryTestApi();

			LandRegistryDataModel model;
			if (titleNumber != null) {
				model = lr.Res(titleNumber, customerId);
				var customer = _customers.Get(customerId);
				var dbLr = new LandRegistry {
					Customer = customer,
					InsertDate = DateTime.UtcNow,
					TitleNumber = titleNumber,
					Request = model.Request,
					Response = model.Response,
					RequestType = model.RequestType,
					ResponseType = model.ResponseType,
				};

				var owners = new List<LandRegistryOwner>();

				if (model.ResponseType == LandRegistryResponseType.Success && model.Res != null && model.Res.Proprietorship != null && model.Res.Proprietorship.ProprietorshipParties != null) {
					foreach (var owner in model.Res.Proprietorship.ProprietorshipParties) {
						owners.Add(new LandRegistryOwner {
							LandRegistry = dbLr,
							FirstName = owner.PrivateIndividualForename,
							LastName = owner.PrivateIndividualSurname,
							CompanyName = owner.CompanyName,
							CompanyRegistrationNumber = owner.CompanyRegistrationNumber,
						});
					}
					dbLr.Owners = owners;
				}
				lrRepo.Save(dbLr);
				landRegistry = dbLr;

				if (model.Attachment != null) {
					var fileRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_AlertDocument>>();
					var doc = new MP_AlertDocument {
						BinaryBody = model.Attachment.AttachmentContent,
						Customer = customer,
						Description = "LandRegistry",
						UploadDate = DateTime.UtcNow,
						DocName = model.Attachment.FileName
					};

					fileRepo.SaveOrUpdate(doc);

					model.Attachment.AttachmentContent = null;
				}
			} else {
				landRegistry = null;
				model = new LandRegistryDataModel {
					Res = new LandRegistryResModel {
						Rejection = new LandRegistryRejectionModel {
							Reason = "Please perform enquiry first to retrieve title number"
						}
					},
					ResponseType = LandRegistryResponseType.None
				};
			}

			model.DataSource = LandRegistryDataSource.Api;
			return model;
		}

		public void GetLandRegistryData(int customerId, List<CustomerAddressModel> addresses) {
			foreach (CustomerAddressModel address in addresses) {
				LandRegistryDataModel model = null;
				if (!string.IsNullOrEmpty(address.HouseName)) {
					model = GetLandRegistryEnquiryData(customerId, null, address.HouseName, null, null,
						address.PostCode);
				} else if (!string.IsNullOrEmpty(address.HouseNumber)) {
					model = GetLandRegistryEnquiryData(customerId, address.HouseNumber, null, null, null,
						address.PostCode);
				} else if (!string.IsNullOrEmpty(address.FlatOrApartmentNumber) &&
							string.IsNullOrEmpty(address.HouseNumber)) {
					model = GetLandRegistryEnquiryData(customerId, address.FlatOrApartmentNumber, null, null, null,
						address.PostCode);
				}

				if (model != null && model.Enquery != null && model.ResponseType == LandRegistryResponseType.Success && model.Enquery.Titles != null &&
					model.Enquery.Titles.Count == 1) {
					LandRegistry dbLandRegistry;
					LandRegistryDataModel landRegistryDataModel = GetLandRegistryData(customerId, model.Enquery.Titles[0].TitleNumber, out dbLandRegistry);

					if (landRegistryDataModel.ResponseType == LandRegistryResponseType.Success) {
						// Verify customer is among owners
						Customer customer = _customers.Get(customerId);
						bool isOwnerAccordingToLandRegistry = IsOwner(customer, landRegistryDataModel.Response, landRegistryDataModel.Res.TitleNumber);
						CustomerAddress dbAdress = customerAddressRepository.Get(address.AddressId);

						dbLandRegistry.CustomerAddress = dbAdress;
						landRegistryRepository.SaveOrUpdate(dbLandRegistry);

						if (isOwnerAccordingToLandRegistry) {
							dbAdress.IsOwnerAccordingToLandRegistry = true;
							customerAddressRepository.SaveOrUpdate(dbAdress);
						}
					}
				} else {
					int num = 0;
					if (model != null && model.Enquery != null && model.Enquery.Titles != null)
						num = model.Enquery.Titles.Count;
					log.WarnFormat(
						"No land registry retrieved for customer id: {5}, house name: {0}, house number: {1}, flat number: {2}, postcode: {3}, num of enquries {4}",
						address.HouseName, address.HouseNumber,
						address.FlatOrApartmentNumber, address.PostCode, num, customerId);
				}
			}
		}

		public LandRegistryDataModel GetLandRegistryEnquiryData(int customerId, string buildingNumber, string buildingName, string streetName, string cityName, string postCode) {
			var lrRepo = ObjectFactory.GetInstance<LandRegistryRepository>();

			try {
				//check cache
				var cache = lrRepo.GetAll()
					.Where(x => x.Customer.Id == customerId && x.RequestType == LandRegistryRequestType.Enquiry);

				if (cache.Any()) {
					foreach (var landRegistry in cache) {
						var lrReq = Serialized.Deserialize<LandRegistryLib.LREnquiryServiceNS.RequestSearchByPropertyDescriptionV2_0Type>(landRegistry.Request);
						var lrAddress = lrReq.Product.SubjectProperty.Address;

						if (AreEqual(lrAddress.BuildingName, buildingName) &&
							AreEqual(lrAddress.BuildingNumber, buildingNumber) &&
							AreEqual(lrAddress.CityName, cityName) &&
							AreEqual(lrAddress.PostcodeZone, postCode) &&
							AreEqual(lrAddress.StreetName, streetName)) {
							var b = new LandRegistryModelBuilder();
							var cacheModel = new LandRegistryDataModel {
								Request = landRegistry.Request,
								Response = landRegistry.Response,
								Enquery = b.BuildEnquiryModel(landRegistry.Response),
								RequestType = landRegistry.RequestType,
								ResponseType = landRegistry.ResponseType,
								DataSource = LandRegistryDataSource.Cache,
							};
							return cacheModel;
						}
					}
				}
			} catch (Exception ex) {
				log.WarnFormat("Failed to retreive land registry enquiry from cache {0}", ex);
			}

			bool isProd = CurrentValues.Instance.LandRegistryProd;

			ILandRegistryApi lr;
			if (isProd) {
				lr = new LandRegistryApi(
					CurrentValues.Instance.LandRegistryUserName,
					Encrypted.Decrypt(CurrentValues.Instance.LandRegistryPassword),
					CurrentValues.Instance.LandRegistryFilePath);
			} else
				lr = new LandRegistryTestApi();

			var model = lr.EnquiryByPropertyDescription(buildingNumber, buildingName, streetName, cityName, postCode, customerId);
			Customer customer = _customers.Get(customerId);

			lrRepo.Save(new LandRegistry {
				Customer = customer,
				InsertDate = DateTime.UtcNow,
				Postcode = string.IsNullOrEmpty(postCode) ? string.Format("{3}{0},{1},{2}", buildingNumber, streetName, cityName, buildingName) : postCode,
				Request = model.Request,
				Response = model.Response,
				RequestType = model.RequestType,
				ResponseType = model.ResponseType
			});
			model.DataSource = LandRegistryDataSource.Api;
			return model;
		}

		public List<Loan> GetOutstandingLoans(int customerId) {
			return loanRepository.ByCustomer(customerId)
				.Where(l => l.Status != LoanStatus.PaidOff)
				.ToList();
		}

		public int GetOutstandingLoansNum(int customerId) {
			return GetOutstandingLoans(customerId)
				.Count;
		}

		public void GetZooplaData(int customerId, bool reCheck = false) {
			new ZooplaStub(customerId, reCheck).Execute();
		}

		public void LinkLandRegistryAndAddress(int customerId, string response, string titleNumber, int landRegistryId) {
			var customer = _customers.Get(customerId);
			var bb = new LandRegistryModelBuilder();
			LandRegistryResModel landRegistryResModel = bb.BuildResModel(response);
			bool isOwnerAccordingToLandRegistry = IsOwner(customer, response, titleNumber);
			if (isOwnerAccordingToLandRegistry) {
				var ownedProperties = new List<CustomerAddress>(customer.AddressInfo.OtherPropertiesAddresses);
				if (customer.PropertyStatus.IsOwnerOfMainAddress) {
					if (customer.AddressInfo.PersonalAddress.Count == 1)
						ownedProperties.Add(customer.AddressInfo.PersonalAddress.First());
				}

				foreach (CustomerAddress ownedProperty in ownedProperties) {
					bool foundMatching = false;
					foreach (LandRegistryAddressModel propertyAddress in landRegistryResModel.PropertyAddresses) {
						if (propertyAddress.PostCode == ownedProperty.Postcode) {
							foundMatching = true;
							break;
						}
					}

					if (foundMatching) {
						ownedProperty.IsOwnerAccordingToLandRegistry = true;
						customerAddressRepository.SaveOrUpdate(ownedProperty);

						LandRegistry dbLandRegistry = landRegistryRepository.Get(landRegistryId);
						dbLandRegistry.CustomerAddress = ownedProperty;
						break;
					}
				}
			}
		}

		public int MarketplaceSeniority(Customer customer) {
			DateTime oMpOriginationDate = customer.GetMarketplaceOriginationDate(oIncludeMp: mp =>
				!mp.Marketplace.IsPaymentAccount ||
				mp.Marketplace.InternalId == PayPal ||
				mp.Marketplace.InternalId == Hmrc
				);

			DateTime oIncorporationDate = GetCustomerIncorporationDate(customer);

			DateTime oDate = (oMpOriginationDate < oIncorporationDate) ? oMpOriginationDate : oIncorporationDate;

			return (int)(DateTime.UtcNow - oDate).TotalDays;
		}

		public void SaveCAISFile(string data, string name, string foldername, int type, int ofItems, int goodUsers, int defaults) {
			_caisReportsHistoryRepository.AddFile(ZipString.Zip(data), name, foldername, type, ofItems, goodUsers, defaults);
		}

		internal List<Loan> GetLastMonthClosedLoans(int customerId) {
			DateTime now = DateTime.UtcNow;
			DateTime startOfMonth = new DateTime(now.Year, now.Month, 1);
			DateTime endOfLastMonth = startOfMonth.Subtract(TimeSpan.FromMilliseconds(1));
			DateTime startOfLastMonth = new DateTime(endOfLastMonth.Year, endOfLastMonth.Month, 1);
			return loanRepository.ByCustomer(customerId)
				.Where(l => l.Status == LoanStatus.PaidOff && l.DateClosed.HasValue && l.DateClosed >= startOfLastMonth && l.DateClosed <= endOfLastMonth)
				.ToList();
		}

		private bool IsOwner(Customer customer, string response, string titleNumber) {
			if (customer == null) {
				log.Warn("IsOwner: returning false because customer is null.");
				return false;
			} // if

			if ((customer.PersonalInfo == null) || string.IsNullOrWhiteSpace(customer.PersonalInfo.Fullname)) {
				log.WarnFormat("IsOwner: returning false for customer {0} because full name is null or empty.", customer.Id);
				return false;
			} // if

			var b = new LandRegistryModelBuilder();
			var lrData = b.BuildResModel(response, titleNumber);

			foreach (ProprietorshipPartyModel proprietorshipParty in lrData.Proprietorship.ProprietorshipParties) {
				// We are taking the first part of the LR first name as it may contain both first and middle name, while we might be missing the middle name
				string firstPartOfFirstName = proprietorshipParty.PrivateIndividualForename;
				int indexOfSpace = firstPartOfFirstName.IndexOf(' ');
				if (indexOfSpace != -1)
					firstPartOfFirstName = firstPartOfFirstName.Substring(0, indexOfSpace);

				string lowerCasedFullName = customer.PersonalInfo.Fullname.ToLower();
				if (lowerCasedFullName.Contains(firstPartOfFirstName.ToLower()) &&
					lowerCasedFullName.Contains(proprietorshipParty.PrivateIndividualSurname.ToLower())
					) {
					// Customer is owner
					return true;
				}
			}

			return false;
		}

		private static readonly Guid Hmrc = new Guid("AE85D6FC-DBDB-4E01-839A-D5BD055CBAEA");
		private static readonly ILog log = LogManager.GetLogger(typeof (StrategyHelper));
		private static readonly Guid PayPal = new Guid("3FA5E327-FCFD-483B-BA5A-DC1815747A28");
		private readonly CaisReportsHistoryRepository _caisReportsHistoryRepository;
		private readonly CustomerAnalyticsRepository _customerAnalytics;
		private readonly CustomerRepository _customers;
		private readonly ISession _session;
		private readonly CustomerAddressRepository customerAddressRepository;
		private readonly LandRegistryRepository landRegistryRepository;
		private readonly LoanRepository loanRepository;
	}
}
