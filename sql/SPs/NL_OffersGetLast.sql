IF OBJECT_ID('NL_OffersGetLast') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE NL_OffersGetLast AS SELECT 1')
END
GO

SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE NL_OffersGetLast
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