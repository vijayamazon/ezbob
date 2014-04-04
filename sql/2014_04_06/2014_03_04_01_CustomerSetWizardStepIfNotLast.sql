IF OBJECT_ID('CustomerSetWizardStepIfNotLast') IS NULL
	EXECUTE('CREATE PROCEDURE CustomerSetWizardStepIfNotLast AS SELECT 1')
GO

ALTER PROCEDURE CustomerSetWizardStepIfNotLast
@CustomerID INT,
@NewStepID INT
AS
BEGIN
	UPDATE Customer SET
		WizardStep = @NewStepID
	FROM
		Customer c
		INNER JOIN WizardStepTypes w
			ON c.WizardStep = w.WizardStepTypeID
			AND w.TheLastOne != 1
	WHERE
		c.Id = @CustomerID
END
GO
