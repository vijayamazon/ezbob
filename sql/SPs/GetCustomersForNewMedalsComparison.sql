IF OBJECT_ID('GetCustomersForNewMedalsComparison') IS NULL
	EXECUTE('CREATE PROCEDURE GetCustomersForNewMedalsComparison AS SELECT 1')
GO

ALTER PROCEDURE GetCustomersForNewMedalsComparison
AS
BEGIN
	SELECT 
		Customer.Id AS CustomerId
	FROM 
		Customer, 
		WizardStepTypes
	WHERE 
		WizardStep = WizardStepTypeID AND 
		TheLastOne = 1 AND 
		(TypeOfBusiness = 'Limited' OR TypeOfBusiness = 'LLP')
END
GO
