namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using ConfigManager;
	using Ezbob.Backend.Strategies.AutoDecisionAutomation;
	using Ezbob.Utils.Security;
	using Ezbob.Utils.Serialization;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using LandRegistryLib;
	using StructureMap;

	public class LandRegistryRes : AStrategy {
		public LandRegistryRes(int customerId, string titleNumber) {
			this.customerID = customerId;
			this.titleNumber = titleNumber;
			DoLinkWithAddress = true;
		} // constructor

		public override string Name { get { return "Land Registry RES"; } } // Name

		public string Result { get; set; }

		public LandRegistry LandRegistry { get; private set; }
		public LandRegistryDataModel RawResult { get; private set; }

		public bool DoLinkWithAddress { get; private set; }

		public virtual void PartialExecute() {
			DoLinkWithAddress = false;
			Execute();
		} // PartialExecute

		public override void Execute() {
			LandRegistry landRegistry;
			RawResult = GetLandRegistryData(out landRegistry);
			LandRegistry = landRegistry;

			if (DoLinkWithAddress)
				LinkLandRegistryAndAddress(this.customerID, landRegistry.Response, this.titleNumber, landRegistry.Id);

			Result = new Serialized(RawResult);

			new SilentAutomation(this.customerID).SetTag(SilentAutomation.Callers.LandRegistry).Execute();
		} // Execute

		public static void LinkLandRegistryAndAddress(int customerId, string response, string titleNumber, int landRegistryId) {
			var customers = ObjectFactory.GetInstance<CustomerRepository>();
			var customerAddressRepository = ObjectFactory.GetInstance<CustomerAddressRepository>();
			var landRegistryRepository = ObjectFactory.GetInstance<LandRegistryRepository>();

			var customer = customers.Get(customerId);

			var bb = new LandRegistryModelBuilder();

			LandRegistryResModel landRegistryResModel = bb.BuildResModel(response);

			bool isOwnerAccordingToLandRegistry = IsOwner(customer, response, titleNumber);

			if (isOwnerAccordingToLandRegistry) {
				var ownedProperties = new List<CustomerAddress>(customer.AddressInfo.OtherPropertiesAddresses);

				if (customer.PropertyStatus.IsOwnerOfMainAddress) {
					if (customer.AddressInfo.PersonalAddress.Count == 1)
						ownedProperties.Add(customer.AddressInfo.PersonalAddress.First());
				} // if

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
				} // for each
			} // if
		} // LinkLandRegistryAndAddress

		public static bool IsOwner(Customer customer, string response, string titleNumber) {
			if (customer == null) {
				Library.Instance.Log.Warn("IsOwner: returning false because customer is null.");
				return false;
			} // if

			if (customer.PersonalInfo == null) {
				Library.Instance.Log.Warn(
					"IsOwner: returning false for customer {0} because personal info is null.",
					customer.Id
				);
				return false;
			} // if

			return IsOwner(customer.Id, customer.PersonalInfo.Fullname, response, titleNumber);
		} // IsOwner

		public static bool IsOwner(int customerID, string customerFullName, string response, string titleNumber) {
			if (string.IsNullOrWhiteSpace(customerFullName)) {
				Library.Instance.Log.Warn(
					"IsOwner: returning false for customer {0} because full name is empty.",
					customerID
				);
				return false;
			} // if

			var b = new LandRegistryModelBuilder();
			var lrData = b.BuildResModel(response, titleNumber);

			if (lrData.Proprietorship == null || lrData.Proprietorship.ProprietorshipParties == null)
				return false;

			string lowerCasedFullName = customerFullName.ToLower();

			foreach (ProprietorshipPartyModel proprietorshipParty in lrData.Proprietorship.ProprietorshipParties) {
				// We are taking the first part of the LR first name as it may contain
				// both first and middle name, while we might be missing the middle name
				if (string.IsNullOrEmpty(proprietorshipParty.PrivateIndividualForename))
					continue;

				if (string.IsNullOrEmpty(proprietorshipParty.PrivateIndividualSurname))
					continue;

				string firstPartOfFirstName = proprietorshipParty.PrivateIndividualForename;
				int indexOfSpace = firstPartOfFirstName.IndexOf(' ');
				if (indexOfSpace != -1)
					firstPartOfFirstName = firstPartOfFirstName.Substring(0, indexOfSpace);

				if (lowerCasedFullName.Contains(firstPartOfFirstName.ToLower()) &&
					lowerCasedFullName.Contains(proprietorshipParty.PrivateIndividualSurname.ToLower())
				) {
					// Customer is owner
					return true;
				} // if
			} // for each

			return false;
		} // IsOwner

		private LandRegistryDataModel GetLandRegistryData(out LandRegistry landRegistry) {
			Log.Debug("GetLandRegistryData begin cId {0} titleNumber {1}", customerID, titleNumber);
			var lrRepo = ObjectFactory.GetInstance<LandRegistryRepository>();

			//check cash
			var cache = lrRepo.GetRes(customerID, titleNumber);
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
				model = lr.Res(titleNumber, customerID);

				var customer = ObjectFactory.GetInstance<CustomerRepository>().Get(customerID);

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
		} // GetLandRegistryData

		private readonly int customerID;
		private readonly string titleNumber;
	} // class LandRegistryRes
} // namespace
