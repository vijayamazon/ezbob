IF OBJECT_ID('dbo.udfGetDiscountPlan') IS NOT NULL 
	DROP FUNCTION dbo.udfGetDiscountPlan
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

-- Returns discount plan by id. If could not find discount plan by id returns
-- any of discount plans which are marked as default. If none is marked as
-- default discount plan with id 1 is returned.

CREATE FUNCTION dbo.udfGetDiscountPlan(@DiscountPlanID INT)
RETURNS @output TABLE (
	DiscountPlanID INT,
	DiscountPlanName NVARCHAR(512),
	ValuesStr NVARCHAR(2048),
	IsDefault BIT,
	ForbiddenForReuse BIT
)
AS
BEGIN
	INSERT INTO @output (
		DiscountPlanID,
		DiscountPlanName,
		ValuesStr,
		IsDefault,
		ForbiddenForReuse
	) SELECT TOP 1
		DiscountPlanID = p.Id,
		DiscountPlanName = p.Name,
		p.ValuesStr,
		p.IsDefault,
		p.ForbiddenForReuse
	FROM
		DiscountPlan p
	WHERE (
			@DiscountPlanID IS NOT NULL AND (
				p.Id = @DiscountPlanID
				OR
				p.IsDefault = 1
				OR
				p.Id = 1
			)
		)
		OR (
			@DiscountPlanID IS NULL AND (
				p.IsDefault = 1
				OR
				p.Id = 1
			)
		)
	ORDER BY
		CASE WHEN @DiscountPlanID IS NOT NULL
			THEN CASE WHEN @DiscountPlanID = p.Id THEN 0 ELSE 1 END
			ELSE 1
		END,		
		p.IsDefault DESC

	RETURN
END
GO
