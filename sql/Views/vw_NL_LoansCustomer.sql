IF OBJECT_ID ('vw_NL_LoansCustomer') IS NOT NULL
	DROP VIEW vw_NL_LoansCustomer
GO

CREATE VIEW [dbo].[vw_NL_LoansCustomer]
AS
SELECT distinct nl.loanID LoanID
,  cr.CustomerID CustomerID

FROM NL_Loans nl 
	JOIN NL_Offers o
		ON o.OfferID = nl.OfferID 
	JOIN NL_Decisions d
		ON  d.DecisionID = o.DecisionID	
	JOIN NL_CashRequests cr
		ON  cr.CashRequestID = d.CashRequestID
GO

