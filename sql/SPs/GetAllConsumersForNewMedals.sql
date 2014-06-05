IF OBJECT_ID('GetAllConsumersForNewMedals') IS NULL
	EXECUTE('CREATE PROCEDURE GetAllConsumersForNewMedals AS SELECT 1')
GO

ALTER PROCEDURE GetBasicCustomerData
AS
BEGIN
	SELECT 
		Customer.Id 
	FROM 
		Customer, 
		WizardStepTypes, 
		Company
	WHERE 
		WizardStep = WizardStepTypeID AND 
		TheLastOne = 1 AND 
		CompanyId = Company.Id AND 
		(Company.TypeOfBusiness = 'Limited' OR Company.TypeOfBusiness = 'LLP') AND
		Customer.Id NOT IN (SELECT CustomerId FROM MP_CustomerMarketPlace WHERE MarketPlaceId IN (SELECT Id FROM MP_MarketplaceType WHERE Name IN ('eBay', 'Amazon', 'EKM', 'Volusion', 'Play', 'Shopify', 'Magento', 'Prestashop', 'Bigcommerce')))
END
GO
