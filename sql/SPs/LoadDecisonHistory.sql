IF OBJECT_ID('LoadDecisionHistory') IS NULL
	EXECUTE('CREATE PROCEDURE LoadDecisionHistory AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadDecisionHistory
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;
	SELECT 
		d.Id DecisionHistoryID,
		isnull(d.Action, cr.UnderwriterDecision) AS Action,
		isnull(d.[Date], cr.UnderwriterDecisionDate) AS 'Date',
		s.FullName AS UnderwriterName,
		l.Name AS LoanType,
		dp.Name AS DiscountPlan,
		ls.LoanSourceName AS LoanSourceName,
		cr.RepaymentPeriod AS RepaymentPeriod,
		cr.InterestRate AS InterestRate,
		isnull(isnull(cr.ManagerApprovedSum, cr.SystemCalculatedSum),0) AS ApprovedSum,
		cr.IsLoanTypeSelectionAllowed AS IsLoanTypeSelectionAllowed,
		cr.Originator AS Originator,
		cr.ManualSetupFeePercent, 
		cr.BrokerSetupFeePercent,
		d.Comment + isnull(dbo.udfGetRejectionReasons(d.Id), '') AS Comment,
		p.Name AS Product,
		pt.Name AS ProductType,
		ft.Name AS FundingType,
		cr.Id AS CashRequestID,
		cr.UnderwriterDecision,
		cr.OfferStart,
		cr.OfferValidUntil,
		cr.ApprovedRepaymentPeriod
	FROM
		CashRequests cr 
	LEFT JOIN 
		DecisionHistory d ON cr.Id = d.CashRequestId
	LEFT JOIN 
		Security_User s ON 	s.UserId = d.UnderwriterId
	LEFT JOIN 
		LoanType l ON l.Id = d.LoanTypeId	
	LEFT JOIN 
		DiscountPlan dp ON dp.Id = cr.DiscountPlanId	
	LEFT JOIN 
		LoanSource ls ON ls.LoanSourceID = cr.LoanSourceID
	LEFT JOIN 
		I_ProductSubType pst ON pst.ProductSubTypeID = cr.ProductSubTypeID	
	LEFT JOIN 
		I_FundingType ft ON ft.FundingTypeID = pst.FundingTypeID	
	LEFT JOIN 
		I_ProductType pt ON pt.ProductTypeID = pst.ProductTypeID
	LEFT JOIN
		I_Product p ON p.ProductID = pt.ProductID		
	WHERE 
		cr.IdCustomer = @CustomerID
END

GO


