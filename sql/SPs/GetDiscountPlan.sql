IF OBJECT_ID('GetDiscountPlan') IS NULL 
	EXECUTE('CREATE PROCEDURE GetDiscountPlan AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Returns discount plan by id. If could not find discount plan by id returns
-- any of discount plans which are marked as default. If none is marked as
-- default discount plan with id 1 is returned.

ALTER PROCEDURE GetDiscountPlan
@DiscountPlanID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT TOP 1
		DiscountPlanID,
		DiscountPlanName,
		ValuesStr,
		IsDefault,
		ForbiddenForReuse
	FROM
		dbo.udfGetDiscountPlan(@DiscountPlanID)
END
GO
