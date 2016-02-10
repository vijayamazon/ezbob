IF OBJECT_ID('GetLastOfferDataForApproval') IS NULL
	EXECUTE('CREATE PROCEDURE GetLastOfferDataForApproval AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetLastOfferDataForApproval
@CustomerId INT
AS
BEGIN
	;WITH last_cr AS (
		SELECT TOP 1
			Id
		FROM
			CashRequests
		WHERE
			IdCustomer = @CustomerID
		ORDER BY
			Id DESC
	), last_banned AS (
		SELECT
			EmailSendingBanned
		FROM 
			CashRequests cr
			INNER JOIN last_cr lcr ON cr.Id = lcr.Id
	) SELECT
		EmailSendingBanned = ISNULL((SELECT EmailSendingBanned FROM last_banned), 0)
END
GO
