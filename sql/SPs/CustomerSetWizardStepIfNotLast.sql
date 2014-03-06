IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerSetWizardStepIfNotLast]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[CustomerSetWizardStepIfNotLast]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CustomerSetWizardStepIfNotLast] 
	(@CustomerID INT,
@NewStepID INT)
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
