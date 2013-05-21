IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_CustomerSignupLoyalty]'))
DROP TRIGGER [dbo].[TR_CustomerSignupLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_CustomerSignupLoyalty
ON Customer
FOR INSERT
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
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'SIGNUP'

	SET NOCOUNT OFF
END
GO
