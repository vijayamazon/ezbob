SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('NL_OffersGetLast') IS NULL
	EXECUTE('CREATE PROCEDURE NL_OffersGetLast AS SELECT 1')
GO

ALTER PROCEDURE  [NL_OffersGetLast]
@CustomerID INT 
AS
BEGIN
	SELECT TOP 1 o.*
	FROM NL_Offers o 
	INNER JOIN NL_Decisions d ON d.DecisionID = o.DecisionID
	INNER JOIN NL_CashRequests cr ON cr.CashRequestID = d.CashRequestID
	WHERE cr.CustomerID=@CustomerID
	ORDER BY o.OfferID DESC
END 

GO


