IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_LoanTakenLoyalty]'))
DROP TRIGGER [dbo].[TR_LoanTakenLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_LoanTakenLoyalty
ON Loan
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, LoanID, ActionID, EarnedPoints)
	SELECT
		CustomerId,
		Id,
		a.ActionID,
		a.Cost * l.LoanAmount
	FROM
		inserted l
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'LOAN'
	
	SET NOCOUNT OFF
END
GO
