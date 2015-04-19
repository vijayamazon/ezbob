IF OBJECT_ID('IovationGetCheckStatus') IS NULL
	EXECUTE('CREATE PROCEDURE IovationGetCheckStatus AS SELECT 1')
GO

ALTER PROCEDURE IovationGetCheckStatus
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @LastCheckDate DATETIME = (SELECT max(Created) FROM FraudIovation WHERE CustomerID = @CustomerID)
	SELECT 
		w.TheLastOne AS FinishedWizard,
		c.FilledByBroker AS FilledByBroker,
		@LastCheckDate AS LastCheckDate
	FROM
		Customer c
		LEFT JOIN WizardStepTypes w ON w.WizardStepTypeID = c.WizardStep
	WHERE
		c.Id = @CustomerID
END

GO
