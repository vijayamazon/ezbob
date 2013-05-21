IF  EXISTS (SELECT * FROM sys.triggers WHERE object_id = OBJECT_ID(N'[dbo].[TR_CustomerLinkAccountLoyalty]'))
DROP TRIGGER [dbo].[TR_CustomerLinkAccountLoyalty]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TRIGGER TR_CustomerLinkAccountLoyalty
ON MP_CustomerMarketPlace
FOR INSERT
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO CustomerLoyaltyProgram (CustomerID, CustomerMarketPlaceID, ActionID, EarnedPoints)
	SELECT
		c.CustomerId,
		c.Id,
		a.ActionID,
		a.Cost
	FROM
		inserted c
		INNER JOIN LoyaltyProgramActions a ON a.ActionName = 'LINKACCOUNT'

	SET NOCOUNT OFF
END
GO
