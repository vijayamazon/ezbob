IF OBJECT_ID('NL_DiscountPlanEntriesGet') IS NULL
	EXECUTE('CREATE PROCEDURE NL_DiscountPlanEntriesGet AS SELECT 1')
GO

ALTER PROCEDURE NL_DiscountPlanEntriesGet
@DiscountPlanID BIGINT
AS
BEGIN
	SET NOCOUNT ON;
	
	SELECT	[DiscountPlanEntryID],	[PaymentOrder], [InterestDiscount], [DiscountPlanID] FROM [dbo].[NL_DiscountPlanEntries] WHERE [DiscountPlanID] = @DiscountPlanID order by [PaymentOrder];
END

GO