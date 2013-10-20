IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAutoApproval]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateAutoApproval]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[UpdateAutoApproval]
	@CustomerId INT,
    @AutoApproveAmount INT

AS
BEGIN
    DECLARE 
		@Score INT,
		@InterestRate NUMERIC(18,4)

	SELECT @Score = ExperianScore
	FROM
		(
			SELECT
				CustomerId,
				ExperianScore,
				ROW_NUMBER() OVER(PARTITION BY CustomerId ORDER BY Id DESC) AS rn
			FROM
				MP_ExperianDataCache
		) as T
	WHERE
		rn = 1 AND
		CustomerId = @CustomerId

	SELECT @InterestRate = LoanIntrestBase FROM BasicInterestRate WHERE FromScore <= @Score AND ToScore >= @Score
	IF @InterestRate IS NULL SET @InterestRate=0

	UPDATE CashRequests SET SystemCalculatedSum=@AutoApproveAmount, InterestRate=@InterestRate WHERE IdCustomer=@CustomerId
	UPDATE Customer SET SystemCalculatedSum = @AutoApproveAmount, CreditSum=@AutoApproveAmount, IsLoanTypeSelectionAllowed=1, LastStatus='Approve' WHERE Id=@CustomerId
END
GO