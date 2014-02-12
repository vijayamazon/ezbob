DELETE FROM ConfigurationVariables WHERE Name='AllowInsertingMobileCodeWithoutGeneration'
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetWizardConfigs]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetWizardConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetWizardConfigs] 
AS
BEGIN
	SELECT
		(SELECT CONVERT(BIT, Value) FROM ConfigurationVariables WHERE Name = 'IsSmsValidationActive') AS IsSmsValidationActive,
		(SELECT CONVERT(INT, Value) FROM ConfigurationVariables WHERE Name = 'NumberOfMobileCodeAttempts') AS NumberOfMobileCodeAttempts
END
GO
