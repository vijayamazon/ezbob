SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM DecisionTrailStepTimeNames WHERE StepTimeNameID = 6)
	INSERT INTO DecisionTrailStepTimeNames (StepTimeNameID, StepTimeName) VALUES (6, 'OldWayFlow')
GO
