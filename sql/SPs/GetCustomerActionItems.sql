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
FROM
	FrequentActionItems,
	FrequentActionItemsForCustomer
WHERE
	FrequentActionItemsForCustomer.CustomerId = @CustomerId AND
	FrequentActionItemsForCustomer.UnmarkedDate IS NULL AND
	FrequentActionItemsForCustomer.ItemId = FrequentActionItems.Id
END
GO
