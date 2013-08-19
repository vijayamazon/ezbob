IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[udfLoanTransactionTimeStatus]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[udfLoanTransactionTimeStatus]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION dbo.udfLoanTransactionTimeStatus (
	@TransactionID INT
)
RETURNS NVARCHAR(6)
AS
BEGIN
	DECLARE @stat TABLE (
		Status NVARCHAR(50) NOT NULL
	)

	INSERT INTO @stat
	SELECT DISTINCT
		StatusBefore
	FROM
		LoanScheduleTransaction
	WHERE
		TransactionID = @TransactionID
	UNION
	SELECT DISTINCT
		StatusAfter
	FROM
		LoanScheduleTransaction
	WHERE
		TransactionID = @TransactionID

	IF EXISTS (SELECT * FROM @stat WHERE Status IN ('Late', 'Paid'))
		RETURN 'Late'

	IF EXISTS (SELECT * FROM @stat WHERE Status IN ('PaidEarly'))
		RETURN 'Early'

	RETURN 'OnTime'
END
GO
