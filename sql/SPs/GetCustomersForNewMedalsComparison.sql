IF OBJECT_ID('GetCustomersForNewMedalsComparison') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomersForNewMedalsComparison AS SELECT 1')
GO

ALTER PROCEDURE GetCustomersForNewMedalsComparison
AS
BEGIN
	SELECT 
		Customer.Id AS CustomerId, MAX(UpdatingEnd) AS CalculationTime
	FROM 
		Customer, 
		WizardStepTypes,
		MP_CustomerMarketPlace
	WHERE 
		WizardStep = WizardStepTypeID AND 
		TheLastOne = 1 AND 
		(TypeOfBusiness = 'Limited' OR TypeOfBusiness = 'LLP') AND
		MP_CustomerMarketPlace.CustomerId = Customer.Id
	GROUP BY Customer.Id
END
GO
