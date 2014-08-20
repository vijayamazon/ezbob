IF OBJECT_ID('GetApprovalData') IS NULL
	EXECUTE('CREATE PROCEDURE GetApprovalData AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetApprovalData
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @NumOfApprovals INT
	
	SELECT
		@NumOfApprovals = COUNT(1)
	FROM
		DecisionHistory
	WHERE
		CustomerId = @CustomerId
	
	SELECT
		@NumOfApprovals AS NumOfApprovals,
		ValidFor,
		ApplyForLoan,
		Id AS CustomerID
	FROM
		Customer
	WHERE
		Id = @CustomerId
END
GO
