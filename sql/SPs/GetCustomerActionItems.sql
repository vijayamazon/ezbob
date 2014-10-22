IF OBJECT_ID('GetCustomerActionItems') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomerActionItems AS SELECT 1')
GO

ALTER PROCEDURE GetCustomerActionItems
@CustomerId INT
AS
BEGIN
	SELECT
		FrequentActionItems.Item,
		FrequentActionItems.MailToCustomer
	INTO
		#GetCustomerActionItemsTmp
	FROM
		FrequentActionItems,
		FrequentActionItemsForCustomer
	WHERE
		FrequentActionItemsForCustomer.CustomerId = @CustomerId AND
		FrequentActionItemsForCustomer.UnmarkedDate IS NULL AND
		FrequentActionItemsForCustomer.ItemId = FrequentActionItems.Id
		
	DECLARE @CostumeActionItem NVARCHAR(1000)
	SELECT @CostumeActionItem = CostumeActionItem FROM Customer WHERE Id = @CustomerId

	IF @CostumeActionItem IS NOT NULL AND @CostumeActionItem != ''
		INSERT INTO #GetCustomerActionItemsTmp VALUES (@CostumeActionItem, 0)

	SELECT Item, MailToCustomer FROM #GetCustomerActionItemsTmp
	
	DROP TABLE #GetCustomerActionItemsTmp
END
GO
