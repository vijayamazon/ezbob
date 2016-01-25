SET QUOTED_IDENTIFIER ON

IF NOT EXISTS (SELECT * FROM LogicalGlueModels WHERE ModelID = 3)
	INSERT INTO LogicalGlueModels (ModelID, ModelName) VALUES (3, 'Logistic regression')
GO
