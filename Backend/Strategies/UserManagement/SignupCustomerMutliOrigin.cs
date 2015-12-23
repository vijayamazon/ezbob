namespace Ezbob.Backend.Strategies.UserManagement {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using System.Web.Security;
	using ConfigManager;
	using Ezbob.Backend.Models;
	using Ezbob.Backend.Strategies.Exceptions;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils.Exceptions;
	using Ezbob.Utils.Security;
	using JetBrains.Annotations;

	public class SignupCustomerMutliOrigin : AStrategy {
		public SignupCustomerMutliOrigin(SignupMultiOriginModel model) {
			this.model = model;
		} // constructor

		public override string Name {
			get { return "SignupCustomerMutliOrigin"; }
		} // Name

		public override void Execute() {
			if (this.model == null)
				throw new StrategyAlert(this, "No sign up data specified.");

			CreateSecurityUserEntry();

			if (Status != MembershipCreateStatus.Success)
				return;

			CreateCustomerEntry();
		} // Execute

		public int UserID { get; private set; }
		public int SessionID { get; private set; }
		public int OriginID { get; private set; }
		public MembershipCreateStatus? Status { get; private set; }

		public string Result {
			get { return Status.HasValue ? Status.Value.ToString() : string.Empty;}
		} // Result

		private void CreateSecurityUserEntry() {
			if (this.model.Origin == null)
				throw new StrategyAlert(this, "No origin specified.");

			if (this.model.PasswordQuestion == null)
				throw new StrategyWarning(this, "No security question specified.");

			try {
				var data = new UserSecurityData(this) {
					Email = this.model.UserName,
					NewPassword = this.model.RawPassword,
					PasswordQuestion = this.model.PasswordQuestion.Value,
					PasswordAnswer = this.model.PasswordAnswer,
				};

				data.ValidateEmail();
				data.ValidateNewPassword();

				var passUtil = new PasswordUtility(CurrentValues.Instance.PasswordHashCycleCount);

				HashedPassword hashedPassword = passUtil.Generate(this.model.UserName, this.model.RawPassword);
				
				var sp = new CreateWebUser(DB, Log) {
					OriginID = (int)this.model.Origin.Value,
					Email = this.model.UserName,
					EzPassword = hashedPassword.Password,
					Salt = hashedPassword.Salt,
					CycleCount = hashedPassword.CycleCount,
					SecurityQuestionID = this.model.PasswordQuestion,
					SecurityAnswer = this.model.PasswordAnswer,
					Ip = this.model.RemoteIp,
				};

				Status = MembershipCreateStatus.ProviderError;

				UserID = 0;

				sp.ForEachRowSafe((sr, bRowsetStart) => {
					if (!sr.ContainsField("UserID"))
						return ActionResult.Continue;

					UserID = sr["UserID"];
					SessionID = sr["SessionID"];
					OriginID = sr["OriginID"];
					return ActionResult.SkipAll;
				});

				if (UserID == -1) {
					Log.Warn("User with email {0} and origin {1} already exists.", this.model.UserName, this.model.Origin);
					Status = MembershipCreateStatus.DuplicateEmail;
				}
				else if (UserID == -2) {
					Log.Warn("Could not find role '{0}'.", sp.RoleName);
					Status = MembershipCreateStatus.ProviderError;
				}
				else if (UserID <= 0) {
					Log.Alert("CreateWebUser returned unexpected result {0}.", UserID);
					Status = MembershipCreateStatus.ProviderError;
				}
				else
					Status = MembershipCreateStatus.Success;
			}
			catch (AException) {
				Status = MembershipCreateStatus.ProviderError;
				throw;
			}
			catch (Exception e) {
				Log.Alert(e, "Failed to create user.");
				Status = MembershipCreateStatus.ProviderError;
			} // try
		} // CreateSecurityUserEntry

		private void CreateCustomerEntry() {
			/*
			var g = new RefNumberGenerator(this.customerRepo);
			var isAutomaticTest = IsAutomaticTest(email);
			var vip = this.vipRequestRepo.RequestedVip(email);
			var whiteLabel = whiteLabelId != 0
				? this.whiteLabelProviderRepo.GetAll().FirstOrDefault(x => x.Id == whiteLabelId)
				: null;

			Broker broker = null;

			if (whiteLabel != null) {
				var brokerRepo = ObjectFactory.GetInstance<BrokerRepository>();
				broker = brokerRepo.GetAll().FirstOrDefault(x => x.WhiteLabel == whiteLabel);
			} // if

			sFirstName = (sFirstName ?? string.Empty).Trim();
			sLastName = (sLastName ?? string.Empty).Trim();

			if (sFirstName == string.Empty)
				sFirstName = null;

			if (sLastName == string.Empty)
				sLastName = null;

			var customer = new Customer {
				Name = email,
				Id = UserID,
				Status = Status.Registered,
				RefNumber = g.GenerateForCustomer(),
				WizardStep = this.dbHelper.WizardSteps.GetAll().FirstOrDefault(x => x.ID == (int)WizardStepType.SignUp),
				CollectionStatus = this.customerStatusRepo.Get((int)CollectionStatusNames.Enabled),
				IsTest = isAutomaticTest,
				IsOffline = null,
				PersonalInfo = new PersonalInfo {
					MobilePhone = mobilePhone,
					MobilePhoneVerified = mobilePhoneVerified,
					FirstName = sFirstName,
					Surname = sLastName,
				},
				TrustPilotStatus = this.dbHelper.TrustPilotStatusRepository.Find(TrustPilotStauses.Neither),
				GreetingMailSentDate = DateTime.UtcNow,
				Vip = vip,
				WhiteLabel = whiteLabel,
				Broker = broker,
			};

			customer.CustomerOrigin = UiCustomerOrigin.Get();

			log.Debug("Customer ({0}): wizard step has been updated to: {1}", customer.Id, (int)WizardStepType.SignUp);
			CampaignSourceRef campaignSourceRef = null;

			if (brokerFillsForCustomer) {
				customer.ReferenceSource = "Broker";
				customer.GoogleCookie = string.Empty;
			} else {
				customer.GoogleCookie = GetAndRemoveCookie("__utmz");
				customer.ReferenceSource = GetAndRemoveCookie("sourceref");
				customer.AlibabaId = GetAndRemoveCookie("alibaba_id");
				customer.IsAlibaba = !string.IsNullOrWhiteSpace(customer.AlibabaId);

				campaignSourceRef = new CampaignSourceRef();
				campaignSourceRef.FContent = GetAndRemoveCookie("fcontent");
				campaignSourceRef.FMedium = GetAndRemoveCookie("fmedium");
				campaignSourceRef.FName = GetAndRemoveCookie("fname");
				campaignSourceRef.FSource = GetAndRemoveCookie("fsource");
				campaignSourceRef.FTerm = GetAndRemoveCookie("fterm");
				campaignSourceRef.FUrl = GetAndRemoveCookie("furl");
				campaignSourceRef.FDate = ToDate(GetAndRemoveCookie("fdate"));
				campaignSourceRef.RContent = GetAndRemoveCookie("rcontent");
				campaignSourceRef.RMedium = GetAndRemoveCookie("rmedium");
				campaignSourceRef.RName = GetAndRemoveCookie("rname");
				campaignSourceRef.RSource = GetAndRemoveCookie("rsource");
				campaignSourceRef.RTerm = GetAndRemoveCookie("rterm");
				campaignSourceRef.RUrl = GetAndRemoveCookie("rurl");
				campaignSourceRef.RDate = ToDate(GetAndRemoveCookie("rdate"));
			} // if

			customer.ABTesting = GetAndRemoveCookie("ezbobab");
			string visitTimes = GetAndRemoveCookie("sourceref_time");
			customer.FirstVisitTime = HttpUtility.UrlDecode(visitTimes);

			if (Request.Cookies["istest"] != null)
				customer.IsTest = true;

			this.customerRepo.Save(customer);

			customer.CustomerRequestedLoan = new List<CustomerRequestedLoan> { new CustomerRequestedLoan {
				CustomerId = customer.Id,
				Amount = ToInt(GetAndRemoveCookie("loan_amount"), customer.CustomerOrigin.GetOrigin() == CustomerOriginEnum.everline ? 24000 : 20000),
				Term = ToInt(GetAndRemoveCookie("loan_period"), customer.CustomerOrigin.GetOrigin() == CustomerOriginEnum.everline ? 12 : 9),
				Created = DateTime.UtcNow,
			}};

			var session = new CustomerSession {
				CustomerId = UserID,
				StartSession = DateTime.UtcNow,
				Ip = RemoteIp(),
				IsPasswdOk = true,
				ErrorMessage = "Registration"
			};
			this.customerSessionRepo.AddSessionIpLog(session);
			Session["UserSessionId"] = session.Id;
			try {
				this.serviceClient.Instance.SaveSourceRefHistory(
					UserID,
					customer.ReferenceSource,
					visitTimes,
					campaignSourceRef
				);
			} catch (Exception e) {
				log.Warn(e, "Failed to save sourceref history.");
			} // try

			// save AlibabaBuyer
			if (customer.AlibabaId != null && customer.IsAlibaba) {
				try {
					AlibabaBuyer alibabaMember = new AlibabaBuyer();
					alibabaMember.AliId = Convert.ToInt64(customer.AlibabaId);
					alibabaMember.Customer = customer;
					EZBob.DatabaseLib.Model.Alibaba.AlibabaBuyerRepository aliMemberRep = ObjectFactory.GetInstance<AlibabaBuyerRepository>();
					aliMemberRep.SaveOrUpdate(alibabaMember);
				} catch (Exception alieException) {
					log.Error(alieException, "Failed to save alibabaMember ID");
				}
			}

			return customer;
			 */
		} // CreateCustomerEntry

		private readonly SignupMultiOriginModel model;

		[SuppressMessage("ReSharper", "MemberCanBePrivate.Local")]
		[SuppressMessage("ReSharper", "ValueParameterNotUsed")]
		[SuppressMessage("ReSharper", "UnusedMember.Local")]
		private class CreateWebUser : AStoredProcedure {
			public CreateWebUser(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) { } // constructor

			public override bool HasValidParameters() {
				if (OriginID <= 0)
					return false;

				if (string.IsNullOrEmpty(Email))
					return false;

				if (string.IsNullOrEmpty(EzPassword))
					return false;

				if (string.IsNullOrEmpty(Salt))
					return false;

				if (string.IsNullOrEmpty(CycleCount))
					return false;

				if (SecurityQuestionID <= 0)
					return false;

				if (string.IsNullOrEmpty(SecurityAnswer))
					return false;

				return true;
			} // HasValidParameters

			public string Email { get; set; }

			public string EzPassword { get; set; }

			public string Salt { get; set; }

			public string CycleCount { get; set; }

			public int? SecurityQuestionID { get; set; }

			public string SecurityAnswer { get; set; }

			public string RoleName {
				get { return UserSecurityData.WebRole; }
				set { }
			} // RoleName

			public int BranchID {
				get { return 0; }
				set { }
			} // BranchID

			public int OriginID { get; set; }

			public string Ip {
				get { return this.ip ?? string.Empty; }
				set { this.ip = value ?? string.Empty; }
			} // Ip

			[UsedImplicitly]
			public DateTime Now {
				get { return DateTime.UtcNow; }
				set { }
			} // Now

			private string ip;
		} // class CreateWebUser
	} // class SignupCustomerMutliOrigin
} // namespace
