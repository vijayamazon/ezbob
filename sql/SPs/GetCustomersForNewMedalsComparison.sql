IF OBJECT_ID('GetCustomersForNewMedalsComparison') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomersForNewMedalsComparison AS SELECT 1')
GO

ALTER PROCEDURE GetCustomersForNewMedalsComparison
AS
BEGIN
	SELECT 
		Customer.Id AS CustomerId, ISNULL(MAX(UpdatingEnd), getutcdate()) AS CalculationTime
	FROM 
		Customer, 
		WizardStepTypes,
		MP_CustomerMarketPlace
	WHERE 
		WizardStep = WizardStepTypeID AND 
		TheLastOne = 1 AND 
		(TypeOfBusiness = 'Limited' OR TypeOfBusiness = 'LLP') AND
		MP_CustomerMarketPlace.CustomerId = Customer.Id AND
		CustomerId NOT IN (SELECT CustomerId AS Num FROM MP_CustomerMarketPlace, MP_MarketplaceType WHERE MarketPlaceId = MP_MarketplaceType.Id AND MP_MarketplaceType.Name = 'HMRC' GROUP BY CustomerId HAVING count(CustomerId) > 1)
	GROUP BY Customer.Id
END
GO
