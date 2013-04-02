using System;
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Email;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database {
        
    public class CustomerMap : ClassMap<Customer> 
	{       
        public CustomerMap() 
		{
			Table("Customer");
            DynamicUpdate();
            Cache.ReadWrite().Region("LongTerm").ReadWrite();
			Id(x => x.Id).GeneratedBy.Assigned().Column("Id");
			Map( x => x.Name ).Not.Nullable();
			HasMany(x => x.CustomerMarketPlaces)
                .AsSet()
                .KeyColumn("CustomerId")
                .Inverse()
                .Cascade.All();
            Map(x => x.MPStatus).Formula(@"CASE WHEN (SELECT COUNT(*) FROM [MP_CustomerMarketPlace] c where c.UpdatingEnd is null and c.CustomerId = Id) > 0 THEN 'not updated' ELSE 'updated' END")
            .Not.Insert().Not.Update();

            References(x => x.LastStartedMainStrategy, "LastStartedMainStrategyId");
            Map(x => x.LastStartedMainStrategyEndTime).CustomType<UtcDateTimeType>();
            Map(x => x.CreditResult).CustomType<CreditResultStatusType>();
            Map(x => x.CreditSum);
            Map(x => x.Status).CustomType<StatusType>();
            Map(x => x.SystemDecision).CustomType<SystemDecisionType>();
            Map(x => x.IsSuccessfullyRegistered, "IsSuccessfullyRegistered");
            Map(x => x.PayPointTransactionId, "PayPointTransactionId").Length(250);
            Map(x => x.Medal, "MedalType").CustomType<MedalType>();
            Map(x => x.GreetingMailSentDate, "GreetingMailSentDate");
            Map(x => x.ApplyCount, "ApplyCount");
            HasMany(x => x.ScoringResults).KeyColumn("CustomerId").Cascade.All();
            
            Map(x => x.DateEscalated).CustomType<UtcDateTimeType>();
            Map(x => x.DateApproved).CustomType<UtcDateTimeType>();
            Map(x => x.UnderwriterName);
            Map(x => x.ManagerName);

            Map(x => x.EscalationReason).Length(200);
            Map(x => x.RejectedReason).Length(200);
            Map(x => x.ApprovedReason).Length(200);

            Map(x => x.Details);
          
            Map(x => x.OfferValidUntil, "ValidFor").CustomType<UtcDateTimeType>();
            Map(x => x.OfferStart, "ApplyForLoan").CustomType<UtcDateTimeType>();
            Map(x => x.CreditCardNo).Length(50);
            Map(x => x.RefNumber).Length(8);
            Map(x => x.PayPointErrorsCount).Nullable();
            Map(x => x.BWAResult).Length(100);
            Map(x => x.AMLResult).Length(100);

            Component(x => x.LimitedInfo, m =>
            {
                m.Map(x => x.LimitedCompanyNumber).Length(255);
                m.Map(x => x.LimitedCompanyName).Length(255);
                m.Map(x => x.LimitedTimeAtAddress);
                m.Map(x => x.LimitedConsentToSearch);
                m.Map(x => x.LimitedBusinessPhone).Length(50);
                m.HasMany(x => x.Directors)
                    .AsSet()
                    .KeyColumn("CustomerId")
                    .Cascade.All()
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();
                m.Map(x => x.LimitedRefNum).Length(250);
            });
            Component(x => x.NonLimitedInfo, m =>
            {
                m.Map(x => x.NonLimitedCompanyName).Length(255);
                m.Map(x => x.NonLimitedTimeInBusiness).Length(255);
                m.Map(x => x.NonLimitedTimeAtAddress);
                m.Map(x => x.NonLimitedBusinessPhone).Length(50);
                m.Map(x => x.NonLimitedConsentToSearch);
                m.HasMany(x => x.Directors)
                    .AsSet()
                    .KeyColumn("CustomerId")
                    .Cascade.All()
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();
                m.Map(x => x.NonLimitedRefNum).Length(250);
            });
            Component(x => x.PersonalInfo, m =>
            {
                m.Map(x => x.FirstName).Length(255);
                m.Map(x => x.MiddleInitial).Length(255);
                m.Map(x => x.Surname).Length(255);
                m.Map(x => x.Fullname).Length(250);
                //m.Map(x => x.Fullname).Formula("(COALESCE(FirstName, '') + ' ' + COALESCE(Surname, '') + ' ' + COALESCE(MiddleInitial, ''))").Length(250);
                m.Map(x => x.DateOfBirth);
                m.Map(x => x.TimeAtAddress);                
                m.Map(x => x.ResidentialStatus).Length(255);
                m.Map(x => x.MobilePhone);
                m.Map(x => x.DaytimePhone);
                m.Map(x => x.Gender).CustomType<GenderType>();
                m.Map(x => x.MartialStatus).CustomType<MartialStatusType>();
                m.Map(x => x.TypeOfBusiness).CustomType<TypeOfBusinessType>();
                m.Map(x => x.OverallTurnOver);
                m.Map(x => x.WebSiteTurnOver);
            });

            Component(x => x.AddressInfo, m =>
            {
                m.HasManyToMany(x => x.PersonalAddress)
                    .AsSet()
                    .Cascade.All()
                    .Table("CustomerAddressRelation")
                    .ParentKeyColumn("customerId")
                    .ChildKeyColumn("addressId")
                    .ChildWhere("addressType=" + Convert.ToInt32(AddressType.PersonalAddress))
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                m.HasManyToMany(x => x.LimitedCompanyAddress)
                    .AsSet()
                    .Cascade.All()
                    .Table("CustomerAddressRelation")
                    .ParentKeyColumn("customerId")
                    .ChildKeyColumn("addressId")
                    .ChildWhere("addressType=" + Convert.ToInt32(AddressType.LimitedCompanyAddress))
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                m.HasManyToMany(x => x.NonLimitedCompanyAddress)
                    .AsSet()
                    .Cascade.All()
                    .Table("CustomerAddressRelation")
                    .ParentKeyColumn("customerId")
                    .ChildKeyColumn("addressId")
                    .ChildWhere("addressType=" + Convert.ToInt32(AddressType.NonLimitedCompanyAddress))
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();


                m.HasManyToMany(x => x.PrevPersonAddresses)
                    .AsSet()
                    .Cascade.All()
                    .Table("CustomerAddressRelation")
                    .ParentKeyColumn("customerId")
                    .ChildKeyColumn("addressId")
                    .ChildWhere("addressType=" + Convert.ToInt32(AddressType.PrevPersonAddresses))
                    .Cache.ReadWrite().Region("LongTerm").ReadWrite();
            });

            Component(x => x.BankAccount, m =>
            {
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

            Map(x => x.Fraud);
            Map(x => x.Eliminated);
            Map(x=> x.Comment,"Comments").CustomType("StringClob").LazyLoad();
            Map(x => x.SetupFee);
            Map(x => x.ReferenceSource).Length(200);
            Map(x => x.EmailState).CustomType<EmailConfirmationRequestStateType>();
            Map(x => x.IsTest);
            References(x => x.CurrentCard, "CurrentDebitCard").Cascade.All();
            Map(x => x.ZohoId);
            Map(x => x.BankAccountValidationInvalidAttempts);

            Component(x => x.CollectionStatus, m =>
            {
                m.Map(x => x.CollectionDescription);
                m.Map(x => x.CollectionFee);
                m.Map(x => x.CollectionDateOfDeclaration).CustomType<UtcDateTimeType>(); 
                m.Map(x => x.IsAddCollectionFee);
                m.Map(x => x.CurrentStatus).Column("CollectionStatus").CustomType(typeof(CollectionStatusType));
            });

            Map(x => x.EbayStatus).Formula(@"dbo.GetMarketPlaceStatus (1, Id)").Not.Insert().Not.Update();
            Map(x => x.AmazonStatus).Formula(@"dbo.GetMarketPlaceStatus (2, Id)").Not.Insert().Not.Update();
            Map(x => x.PayPalStatus).Formula(@"dbo.GetMarketPlaceStatus (3, Id)").Not.Insert().Not.Update();
            Map(x => x.WizardStep).CustomType(typeof(WizardStepType));

		}
    }
}
