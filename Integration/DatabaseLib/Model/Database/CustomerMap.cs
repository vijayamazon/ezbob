using System;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
	public class CustomerMap : ClassMap<Customer> {
		public CustomerMap() {
			Table("Customer");
			DynamicUpdate();

			Cache.ReadWrite().Region("LongTerm").ReadWrite();

			Id(x => x.Id);
			Map(x => x.Name).Not.Nullable();

			HasMany(x => x.CustomerMarketPlaces)
				.AsSet()
				.KeyColumn("CustomerId")
				.Inverse()
				.Cascade.All();

			References(x => x.CurrentCard, "CurrentDebitCard").Cascade.All();
			Map(x => x.IsWasLate);
			Map(x => x.LastStartedMainStrategyEndTime).CustomType<UtcDateTimeType>();
			Map(x => x.CreditResult).CustomType<CreditResultStatusType>();
			Map(x => x.CreditSum);
			Map(x => x.IsLoanTypeSelectionAllowed);
			Map(x => x.Status).CustomType<StatusType>();
			Map(x => x.SystemDecision).CustomType<SystemDecisionType>();
			Map(x => x.Medal, "MedalType").CustomType<MedalType>();
			Map(x => x.GreetingMailSentDate, "GreetingMailSentDate");
			Map(x => x.ApplyCount, "ApplyCount");
			
			Map(x => x.DateEscalated).CustomType<UtcDateTimeType>();
			Map(x => x.DateApproved).CustomType<UtcDateTimeType>();
			Map(x => x.DateRejected).CustomType<UtcDateTimeType>();
			Map(x => x.UnderwriterName);
			Map(x => x.ManagerName);

			Map(x => x.EscalationReason).Length(200);
			Map(x => x.RejectedReason).Length(1000);
			Map(x => x.ApprovedReason).Length(200);

			Map(x => x.Details);
			Map(x => x.PendingStatus).CustomType<PendingStatusType>();

			Map(x => x.OfferValidUntil, "ValidFor").CustomType<UtcDateTimeType>();
			Map(x => x.OfferStart, "ApplyForLoan").CustomType<UtcDateTimeType>();
			
			Map(x => x.RefNumber).Length(8);
			Map(x => x.PayPointErrorsCount).Nullable();
			Map(x => x.BWAResult).Length(100);
			Map(x => x.AMLResult).Length(100);
			Map(x => x.AmlScore);
			Map(x => x.AmlDescription).Length(200);
			
			Component(x => x.PersonalInfo, m => {
				m.Map(x => x.FirstName).Length(255);
				m.Map(x => x.MiddleInitial).Length(255);
				m.Map(x => x.Surname).Length(255);
				m.Map(x => x.Fullname).Length(250);
				m.Map(x => x.DateOfBirth);
				m.Map(x => x.TimeAtAddress);
				m.Map(x => x.MobilePhone);
				m.Map(x => x.MobilePhoneVerified);
				m.Map(x => x.DaytimePhone);
				m.Map(x => x.Gender).CustomType<GenderType>();
				m.Map(x => x.MaritalStatus).CustomType<MaritalStatusType>();
				m.Map(x => x.TypeOfBusiness).CustomType<TypeOfBusinessType>();
				m.Map(x => x.IndustryType).CustomType<IndustryTypeType>();
				m.Map(x => x.OverallTurnOver);
				m.Map(x => x.WebSiteTurnOver);
			});

			Component(x => x.AddressInfo, m => {
				m.HasMany(x => x.PersonalAddress)
				 .AsSet()
				 .KeyColumn("customerId")
				 .Where("addressType=" + Convert.ToInt32(CustomerAddressType.PersonalAddress))
				 .Cascade.AllDeleteOrphan()
				 .Inverse()
				 .Cache.ReadWrite().Region("LongTerm").ReadWrite();

				m.HasMany(x => x.PrevPersonAddresses)
				 .AsSet()
				 .KeyColumn("customerId")
				 .Where("addressType=" + Convert.ToInt32(CustomerAddressType.PrevPersonAddresses))
				 .Cascade.AllDeleteOrphan()
				 .Inverse()
				 .Cache.ReadWrite().Region("LongTerm").ReadWrite();

				m.HasMany(x => x.AllAddresses)
				 .AsSet()
				 .KeyColumn("customerId")
				 .Cascade.All()
				 .Inverse()
				 .Cache.ReadWrite().Region("LongTerm").ReadWrite();

				m.HasMany(x => x.OtherPropertiesAddresses)
				 .AsSet()
				 .KeyColumn("customerId")
				 .Where("addressType=" + Convert.ToInt32(CustomerAddressType.OtherPropertyAddress))
				 .Cascade.AllDeleteOrphan()
				 .Inverse()
				 .Cache.ReadWrite().Region("LongTerm").ReadWrite();
			});

			Component(x => x.BankAccount, m => {
				m.Map(x => x.AccountNumber).Length(8);
				m.Map(x => x.SortCode).Length(8);
				m.Map(x => x.Type, "BankAccountType").CustomType<BankAccountTypeType>();
			});

			HasMany(m => m.Loans)
				.AsSet()
				.KeyColumn("CustomerId")
				.Cascade.All()
				.Inverse()
				.Cache.ReadWrite().Region("LongTerm").ReadWrite();

			HasMany(m => m.CashRequests)
				.AsSet()
				.KeyColumn("IdCustomer")
				.OrderBy("CreationDate")
				.Cascade.All();

			HasMany(m => m.PayPointCards)
				.AsSet()
				.KeyColumn("CustomerId")
				.OrderBy("DateAdded")
				.Inverse()
				.Cascade.All();

			HasMany(m => m.BankAccounts)
				.AsSet()
				.KeyColumn("CustomerId")
				.Inverse()
				.Cascade.All();

			HasMany(m => m.DecisionHistory)
				.AsSet()
				.KeyColumn("CustomerId")
				.Inverse()
				.Cascade.All();

			HasMany(m => m.ActiveCampaigns)
				.AsSet()
				.KeyColumn("CustomerId")
				.Inverse()
				.Cascade.All();

			Map(x => x.Fraud);
			Map(x => x.FraudStatus).CustomType<FraudStatus>();

			Map(x => x.Comment, "Comments").CustomType("StringClob").LazyLoad();
			Map(x => x.SetupFee);
			Map(x => x.ReferenceSource).Length(200);
			Map(x => x.IsTest);
			Map(x => x.IsAvoid, "AvoidAutomaticDescison");
			Map(x => x.IsOffline);
			Map(x => x.IsDirector);
			Map(x => x.ConsentToSearch);
			Map(x => x.BankAccountValidationInvalidAttempts);
			Map(x => x.ABTesting).Length(512);

			Map(x => x.CollectionDescription).Length(500);
			References(x => x.CollectionStatus, "CollectionStatus");
			
			References(x => x.WizardStep, "WizardStep");

			Map(x => x.NumApproves);
			Map(x => x.NumRejects);
			Map(x => x.SystemCalculatedSum);
			Map(x => x.ManagerApprovedSum);
			Map(x => x.LastStatus);
			Map(x => x.TotalPrincipalRepaid);

			Map(x => x.FirstLoanDate).CustomType<UtcDateTimeType>();
			Map(x => x.LastLoanDate).CustomType<UtcDateTimeType>();
			Map(x => x.LastLoanAmount);
			Map(x => x.AmountTaken);

			Map(x => x.PromoCode);
			Map(x => x.MonthlyStatusEnabled);

			HasMany(x => x.CustomerRequestedLoan)
				.AsBag()
				.KeyColumn("CustomerId")
				.Cascade.AllDeleteOrphan()
				.Inverse();

			HasMany(x => x.Session)
				.AsBag()
				.KeyColumn("CustomerId")
				.Cascade.All()
				.Inverse();

			References(x => x.Company, "CompanyId").Cascade.All();

			Map(x => x.CciMark);
			Map(x => x.GoogleCookie).Length(300);

			References(x => x.TrustPilotStatus, "TrustPilotStatusID");
			References(x => x.ExternalCollectionStatus, "ExternalCollectionStatusID");

			References(x => x.QuickOffer, "QuickOfferID").Nullable().Cascade.All();

			References(x => x.Broker, "BrokerID").Cascade.None();
			References(x => x.WhiteLabel, "WhiteLabelId").Cascade.All();
			
			Map(x => x.FilledByBroker);
			Map(x => x.Vip);
			Map(x => x.DefaultCardSelectionAllowed);

			HasMany(x => x.CustomerRelationStates)
				.AsBag()
				.KeyColumn("CustomerId")
				.Cascade.All()
				.Inverse();
			
			HasMany(x => x.CompanyFiles)
				.AsBag()
				.KeyColumn("CustomerId")
				.Cascade.All()
				.Inverse();
			
			Map(x => x.FirstVisitTime).Length(64);
			Map(x => x.ExperianConsumerScore);
			References(x => x.PropertyStatus, "PropertyStatusId").Cascade.All();

			Map(x => x.IsWaitingForSignature);

			Map(x => x.CostumeActionItem).Length(1000);
			Map(x => x.BlockTakingLoan);
			Map(x => x.IsAlibaba);
			Map(x => x.AlibabaId).Length(300);

			References(x => x.CustomerOrigin, "OriginID").Cascade.All();

			HasOne(x => x.CampaignSource)
				.PropertyRef(p => p.Customer)
				.Cascade.All();

			Map(x => x.HasApprovalChance);
		} // constructor
	} // class CustomerMap
} // namespace EZBob.DatabaseLib.Model.Database