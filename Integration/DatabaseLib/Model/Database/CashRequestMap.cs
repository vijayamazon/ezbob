using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
    public class CashRequestMap : ClassMap<CashRequest> 
    {
        public CashRequestMap()
        {
            Table("CashRequests");
            LazyLoad();
            Id(x => x.Id);
            Map(x => x.HasLoans);
            References(x => x.Customer, "IdCustomer");
            Map(x => x.IdUnderwriter);
            Map(x => x.IdManager);
            Map(x => x.CreationDate);
            Map(x => x.SystemDecision).CustomType<SystemDecisionType>(); ;
            Map(x => x.UnderwriterDecision).CustomType<CreditResultStatusType>();
            Map(x => x.ManagerDecision).CustomType<CreditResultStatusType>();
            Map(x => x.SystemDecisionDate);
            Map(x => x.UnderwriterDecisionDate);
            Map(x => x.ManagerDecisionDate);
            Map(x => x.EscalatedDate);
            Map(x => x.SystemCalculatedSum);
            Map(x => x.ManagerApprovedSum);
            Map(x => x.MedalType).CustomType<MedalType>();
            Map(x => x.EscalationReason);
            Map(x => x.InterestRate);
            Map(x => x.APR);
            Map(x => x.RepaymentPeriod);
            Map(x => x.UseSetupFee);
            Map(x => x.ZohoId).Length(100);
            Map(x => x.OfferStart);
            Map(x => x.OfferValidUntil);
            Map(x => x.EmailSendingBanned);
            Map(x => x.UnderwriterComment).Length(200);
            References(x => x.LoanType, "LoanTypeId");
            Map(x => x.LoanTemplate).CustomType("StringClob");
        }
    }
}
