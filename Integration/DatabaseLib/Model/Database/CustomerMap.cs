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
            Map(x => x.Name).Not.Nullable();
            HasMany(x => x.CustomerMarketPlaces)
                .AsSet()
                .KeyColumn("CustomerId")
                .Inverse()
                .Cascade.All();
            References(x => x.CurrentCard, "CurrentDebitCard").Cascade.All();
            Map(x => x.PayPointTransactionId, "PayPointTransactionId").Length(250);
            References(x => x.LastStartedMainStrategy, "LastStartedMainStrategyId");
            Map(x => x.LastStartedMainStrategyEndTime).CustomType<UtcDateTimeType>();
            Map(x => x.CreditResult).CustomType<CreditResultStatusType>();
            Map(x => x.CreditSum);
            Map(x => x.IsLoanTypeSelectionAllowed);
            Map(x => x.Status).CustomType<StatusType>();
            Map(x => x.SystemDecision).CustomType<SystemDecisionType>();
            Map(x => x.IsSuccessfullyRegistered, "IsSuccessfullyRegistered");
            Map(x => x.Medal, "MedalType").CustomType<MedalType>();
            Map(x => x.GreetingMailSentDate, "GreetingMailSentDate");
            Map(x => x.ApplyCount, "ApplyCount");
            HasMany(x => x.ScoringResults).KeyColumn("CustomerId").Cascade.All();

            Map(x => x.DateEscalated).CustomType<UtcDateTimeType>();
            Map(x => x.DateApproved).CustomType<UtcDateTimeType>();
            Map(x => x.DateRejected).CustomType<UtcDateTimeType>();
            Map(x => x.UnderwriterName);
            Map(x => x.ManagerName);

            Map(x => x.EscalationReason).Length(200);
			Map(x => x.RejectedReason).Length(200);
			Map(x => x.ApprovedReason).Length(200);

            Map(x => x.Details);
            Map(x => x.PendingStatus).CustomType<PendingStatusType>();

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
                    m.HasMany(x => x.PersonalAddress)
                     .AsSet()
                     .KeyColumn("customerId")
                     .Where("addressType=" + Convert.ToInt32(CustomerAddressType.PersonalAddress))
                     .Cascade.All()
                     .Inverse()
                     .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                    m.HasMany(x => x.PrevPersonAddresses)
                     .AsSet()
                     .KeyColumn("customerId")
                     .Where("addressType=" + Convert.ToInt32(CustomerAddressType.PrevPersonAddresses))
                     .Cascade.All()
                     .Inverse()
                     .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                    m.HasMany(x => x.LimitedCompanyAddress)
                     .AsSet()
                     .KeyColumn("customerId")
                     .Where("addressType=" + Convert.ToInt32(CustomerAddressType.LimitedCompanyAddress))
                     .Cascade.All()
                     .Inverse()
                     .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                    m.HasMany(x => x.LimitedCompanyAddressPrev)
                     .AsSet()
                     .KeyColumn("customerId")
                     .Where("addressType=" + Convert.ToInt32(CustomerAddressType.LimitedCompanyAddressPrev))
                     .Cascade.All()
                     .Inverse()
                     .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                    m.HasMany(x => x.NonLimitedCompanyAddress)
                     .AsSet()
                     .KeyColumn("customerId")
                     .Where("addressType=" + Convert.ToInt32(CustomerAddressType.NonLimitedCompanyAddress))
                     .Cascade.All()
                     .Inverse()
                     .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                    m.HasMany(x => x.NonLimitedCompanyAddressPrev)
                     .AsSet()
                     .KeyColumn("customerId")
                     .Where("addressType=" + Convert.ToInt32(CustomerAddressType.NonLimitedCompanyAddressPrev))
                     .Cascade.All()
                     .Inverse()
                     .Cache.ReadWrite().Region("LongTerm").ReadWrite();

                    m.HasMany(x => x.AllAddresses)
                     .AsSet()
                     .KeyColumn("customerId")
                     .Cascade.All()
                     .Inverse()
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

            HasMany(m => m.DecisionHistory)
                .AsSet()
                .KeyColumn("CustomerId")
                .Inverse()
                .Cascade.All();

            Map(x => x.Fraud);
            Map(x => x.FraudStatus).CustomType<FraudStatus>();
            Map(x => x.FinancialAccounts);
            Map(x => x.Eliminated);
            Map(x => x.Comment, "Comments").CustomType("StringClob").LazyLoad();
            Map(x => x.SetupFee);
            Map(x => x.ReferenceSource).Length(200);
            Map(x => x.EmailState).CustomType<EmailConfirmationRequestStateType>();
            Map(x => x.IsTest);
            Map(x => x.IsAvoid, "AvoidAutomaticDescison");
            Map(x => x.ZohoId);
            Map(x => x.BankAccountValidationInvalidAttempts);
            Map(x => x.ABTesting).Length(512);

            Component(x => x.CollectionStatus, m =>
                {
                    m.Map(x => x.CollectionDescription);
                    m.Map(x => x.CollectionFee);
                    m.Map(x => x.CollectionDateOfDeclaration).CustomType<UtcDateTimeType>();
                    m.Map(x => x.IsAddCollectionFee);
                    m.Map(x => x.CurrentStatus).Column("CollectionStatus").CustomType(typeof (CollectionStatusType));
                });
            Map(x => x.WizardStep).CustomType(typeof (WizardStepType));

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

            //for better performance some calculated field take out into formula
            Map(x => x.EbayStatus).Formula(@"dbo.GetMarketPlaceStatus (1, Id)").Not.Insert().Not.Update();
            Map(x => x.AmazonStatus).Formula(@"dbo.GetMarketPlaceStatus (2, Id)").Not.Insert().Not.Update();
			Map(x => x.PayPalStatus).Formula(@"dbo.GetMarketPlaceStatus (3, Id)").Not.Insert().Not.Update();
			Map(x => x.EkmStatus).Formula(@"dbo.GetMarketPlaceStatus (4, Id)").Not.Insert().Not.Update();
			Map(x => x.VolusionStatus).Formula(@"dbo.GetMarketPlaceStatusByName ('Volusion', Id)").Not.Insert().Not.Update();
			Map(x => x.PayPointStatus).Formula(@"dbo.GetMarketPlaceStatusByName ('PayPoint', Id)").Not.Insert().Not.Update();
			Map(x => x.PlayStatus).Formula(@"dbo.GetMarketPlaceStatusByName ('Play', Id)").Not.Insert().Not.Update();
			Map(x => x.YodleeStatus).Formula(@"dbo.GetMarketPlaceStatusByName ('Yodlee', Id)").Not.Insert().Not.Update();
			Map(x => x.FreeAgentStatus).Formula(@"dbo.GetMarketPlaceStatusByName ('FreeAgent', Id)").Not.Insert().Not.Update();
			Map(x => x.SageStatus).Formula(@"dbo.GetMarketPlaceStatusByName ('Sage', Id)").Not.Insert().Not.Update();
			Map(x => x.ShopifyStatus).Formula(@"dbo.GetMarketPlaceStatusByName ('Shopify', Id)").Not.Insert().Not.Update();
			Map(x => x.XeroStatus).Formula(@"dbo.GetMarketPlaceStatusByName ('Xero', Id)").Not.Insert().Not.Update();
            Map(x => x.MPStatus)
                .Formula(
                    @"CASE WHEN (SELECT COUNT(*) FROM [MP_CustomerMarketPlace] c where c.UpdatingEnd is null and c.CustomerId = Id) > 0 THEN 'not updated' ELSE 'updated' END")
                .Not.Insert().Not.Update();
            Map(x => x.MpList).Formula(@"dbo.MP_List (Id)").Not.Insert().Not.Update();
            
            Map(x => x.OutstandingBalance)
                .Formula("(select ISNULL(sum(l.Balance), 0) from [Loan] l where l.CustomerId = Id)")
                .Not.Insert()
                .Not.Update();

            Map(x => x.Delinquency)
                .Formula(
                    @"(select DATEDIFF(day, ISNULL(MIN(s.[Date]), GETUTCDATE()), GETUTCDATE()) from [Loan] l left join [LoanSchedule] s
                on l.Id = s.LoanId 
                where l.[CustomerId] = Id and s.[Date] <= GETUTCDATE() and s.[Status] = 'Late')")
                .Not.Insert()
                .Not.Update();

            Map(x=>x.NextRepaymentDate)
                .Formula(@"(select top 1 s.[Date] from [LoanSchedule] s left join [loan] l
                        on l.[Id] = s.[LoanId]
                        where l.[CustomerId] = Id and s.[Status] in ('StillToPay','Late')
                        order by s.[Date])")
                .Not.Insert()
                .Not.Update();

            Map(x => x.DateOfLate)
                .Formula(
                    @"(select MIN(s.[Date]) from [Loan] l left join [LoanSchedule] s
                     on l.Id = s.[LoanId] 
                     where l.[CustomerId] = Id and s.[Date] <= GETUTCDATE() and s.[Status] = 'Late')")
                .Not.Insert()
                .Not.Update();
            Map(x => x.OfferDate)
                .Formula(
                    @"(select MAX(l.[UnderwriterDecisionDate])from [CashRequests] l where l.IdCustomer = Id)")
                .Not.Insert()
                .Not.Update();
            Map(x => x.LatestCRMstatus)
                .Formula(
                @"(SELECT TOP 1 ST.NAME FROM [CustomerRelations] AS CR 
                    LEFT JOIN [CRMStatuses] AS ST ON CR.StatusId = ST.Id
                    WHERE CR.CustomerId=Id AND ST.Id != 1 ORDER BY CR.Timestamp DESC)")
                .Not.Insert()
                .Not.Update();
            Map(x => x.AmountOfInteractions)
                .Formula(
                    @"(SELECT COUNT(*) FROM [CashRequests] cr
                    WHERE (GETUTCDATE() - CR.[CreationDate])<5
                    and CR.IdCustomer=Id)")
                .Not.Insert()
                .Not.Update();
            Map(x=>x.LateAmount)
                .Formula(@"(select ISNULL(SUM(s.[LoanRepayment]),0) from [LoanSchedule] s left join [Loan] l 
                        on l.[Id] = s.[LoanId]
                        where s.[Status] like 'Late' and l.[CustomerId] = Id)")
                .Not.Insert()
                .Not.Update();
            Map(x => x.CustomerStatus)
                .Formula("CreditResult")
                .Not.Insert()
                .Not.Update();
        }
	}
}

