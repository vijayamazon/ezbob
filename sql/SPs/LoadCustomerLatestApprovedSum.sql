IF OBJECT_ID('LoadCustomerLatestApprovedSum') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerLatestApprovedSum AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerLatestApprovedSum
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	------------------------------------------------------------------------------

	DECLARE @ApprovedSum INT = 0
	
	------------------------------------------------------------------------------

	SELECT TOP 1
		@ApprovedSum = CASE
			WHEN cr.UnderwriterDecision = 'Approved' THEN cr.ManagerApprovedSum
			ELSE cr.SystemCalculatedSum
		END
	FROM
		CashRequests cr
	WHERE
		(
			cr.UnderwriterDecision = 'Approved'
			OR
			(
				cr.UnderwriterDecision IS NULL
				AND
				cr.SystemDecision = 'Approve'
			)
		)
		AND
		cr.IdCustomer = @CustomerID
	ORDER BY
		cr.CreationDate DESC,
		cr.Id DESC
	
	------------------------------------------------------------------------------

	SELECT @ApprovedSum AS ApprovedSum
END
GO
