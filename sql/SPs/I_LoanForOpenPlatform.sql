SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('I_LoanForOpenPlatform') IS NULL
	EXECUTE('CREATE PROCEDURE I_LoanForOpenPlatform AS SELECT 1')
GO

ALTER PROCEDURE I_LoanForOpenPlatform
@LoanID INT
AS
BEGIN
	DECLARE @Result BIT = 0

	SELECT
		@Result = CAST((CASE WHEN count(*) = 0 THEN 0 ELSE 1 END) AS BIT)
	FROM 
		Loan l 
	INNER JOIN 
		CashRequests cr ON l.RequestCashId = cr.Id 
	INNER JOIN 
		I_OpenPlatformOffer o ON o.CashRequestID = cr.Id
	WHERE 
		l.Id = @LoanID	

	SELECT ISNULL(@Result, 0) AS IsForOpenPlatform
END
GO
