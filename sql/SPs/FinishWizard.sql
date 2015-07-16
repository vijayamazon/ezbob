IF OBJECT_ID('FinishWizard') IS NULL
	EXECUTE('CREATE PROCEDURE FinishWizard AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE FinishWizard
@CustomerId INT
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @LastWizardStep INT

	SELECT @LastWizardStep = WizardStepTypeID FROM WizardStepTypes WHERE TheLastOne = 1

	UPDATE Customer SET WizardStep = @LastWizardStep WHERE Id = @CustomerId

	INSERT INTO ExperianConsentAgreement(Template, CustomerId, FilePath)
		VALUES ('', @CustomerId, '')
END
GO
