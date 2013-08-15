IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_RepaymentLoyalty]'))
DROP TRIGGER [dbo].[TR_RepaymentLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_RepaymentLoyalty
ON LoanSchedule
FOR UPDATE
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, LoanID, LoanScheduleID, ActionID, EarnedPoints)
	SELECT
		l.CustomerId,
		l.Id,
		i.Id,
		a.ActionID,
		a.Cost * CAST(d.LoanRepayment - i.LoanRepayment AS NUMERIC(29, 0))
	FROM
		deleted d
		INNER JOIN inserted i ON d.Id = i.id
		INNER JOIN Loan l ON d.LoanId = l.Id
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'REPAYMENT'
	WHERE
		d.Status != 'Late'
		AND
		i.Status != 'Late'
	
	SET NOCOUNT OFF
END
GO
