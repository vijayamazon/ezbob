SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

-- SortOrder field defines order of entries (ORDER BY SortOrder ASC, DiscountPlanEntryID DESC)
-- and not number of repayment period. Entry is always related to repetition period. I.e. if
-- the same plan is applied to monthly repaid loan and to weekly repaid loan and an entry in
-- the second position says "-10%" that means that in the former case customer receives 10%
-- discound for the second month while in the latter case customer receives 10% discount for
-- the second week.
-- Value "0.1" in InterestRateDelta means "10%", value "-0.05" means "-5%".

BEGIN TRY
	DROP TABLE #discountplanTemp
END TRY
BEGIN CATCH
END CATCH

DECLARE @Id INT
DECLARE @NL_Id INT
DECLARE @Name NVARCHAR(50)
DECLARE @ValuesStr NVARCHAR(100)
DECLARE @IsDefault BIT
Declare @ForbiddenForReuse BIT
Declare @Percent FLOAT

SELECT
	Id,
	Name,
	ValuesStr,
	IsDefault,
	ForbiddenForReuse
INTO
	#discountplanTemp
FROM
	DiscountPlan

SET @Percent = 100.00

-- Чего только люди не придумают чтобы не использовать CURSOR...

WHILE EXISTS (SELECT * FROM #discountplanTemp)
BEGIN
	SELECT TOP 1
		@Name = Name,
		@Id = Id,
		@VALUESStr = VALUESStr,
		@IsDefault = IsDefault,
		@ForbiddenForReuse = ForbiddenForReuse
	FROM
		#discountplanTemp

	IF (SELECT DiscountPlan FROM NL_DiscountPlans WHERE DiscountPlan = @Name ) IS NULL
		INSERT INTO NL_DiscountPlans (DiscountPlanID, DiscountPlan, IsDefault, IsActive) VALUES (@Id, LTRIM(RTRIM(@Name)), @IsDefault, @ForbiddenForReuse)

	SELECT @NL_Id = DiscountPlanID FROM NL_DiscountPlans WHERE DiscountPlan = @Name
	
	IF @NL_Id IS NOT NULL
	BEGIN
		IF (SELECT COUNT(DiscountPlanEntryID) FROM NL_DiscountPlanEntries WHERE DiscountPlanID = @NL_Id group by DiscountPlanID) IS NULL
		BEGIN
			INSERT INTO NL_DiscountPlanEntries (DiscountPlanID, PaymentOrder, InterestDiscount)
			SELECT
				@NL_Id,
				splitted.Id,
				CAST(splitted.Data AS float) / @Percent
			FROM
				dbo.udfSplit(@VALUESStr, ',') AS splitted
		END
	END

	DELETE FROM #discountplanTemp WHERE ID = @Id
END

DROP TABLE #discountplanTemp
GO
