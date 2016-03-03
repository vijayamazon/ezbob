IF OBJECT_ID('NL_MigrationLoanLegals') IS NULL
	EXECUTE('CREATE PROCEDURE NL_MigrationLoanLegals AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[NL_MigrationLoanLegals]	
AS
BEGIN
	
	SET NOCOUNT ON;

    SELECT 
		ll.Id as LegalID,--ll.BrokerSetupFeePercent,ll.CashRequestsId, ll.Created as LLcreated, ll.SignedName,
		--c.Id as CRID, --c.CreationDate as CRcreated, c.IdCustomer,c.OfferStart, c.OfferValidUntil, 
		--nlc.OldCashRequestID, 
		--nlc.CashRequestID, nlc.CustomerID, --nlc.RequestTime,
		--d.DecisionID, 
		o.OfferID
	from 
		LoanLegal ll join [dbo].[CashRequests] c on c.Id=ll.CashRequestsId join NL_CashRequests nlc on nlc.OldCashRequestID=c.Id
		join [dbo].[NL_Decisions] d on d.CashRequestID=nlc.CashRequestID
		join [dbo].[NL_Offers] o on d.DecisionID=o.DecisionID
		left join [dbo].[NL_LoanLegals] nll on nll.OfferID=o.OfferID
	where 
		d.DecisionID = (select MAX(DecisionID) from [dbo].[NL_Decisions] where CashRequestID=nlc.CashRequestID and DecisionNameID in (select DecisionID from Decisions where [DecisionName] in ('Approve', 'ReApprove')))
		and o.OfferID = (select MAX(OfferID) from [dbo].[NL_Offers] where DecisionID=d.DecisionID)
		and nll.OfferID is null
END
