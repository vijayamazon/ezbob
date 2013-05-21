IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_CustomerPersonalInfoLoyalty]'))
DROP TRIGGER [dbo].[TR_CustomerPersonalInfoLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_CustomerPersonalInfoLoyalty
ON Customer
FOR UPDATE
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, ActionID, EarnedPoints)
	SELECT
		c.Id,
		a.ActionID,
		a.Cost
	FROM
		inserted c
		INNER JOIN deleted d ON c.Id = d.Id AND c.IsSuccessfullyRegistered = 1 AND d.IsSuccessfullyRegistered = 0
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'PERSONALINFO'

	SET NOCOUNT OFF
END
GO
