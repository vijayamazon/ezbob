IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserManagementConfigs]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUserManagementConfigs]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserManagementConfigs]
AS
BEGIN
	SELECT
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'LoginValidationStringForWeb') AS LoginValidationStringForWeb,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'NumOfInvalidPasswordAttempts') AS NumOfInvalidPasswordAttempts,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'InvalidPasswordAttemptsPeriodSeconds') AS InvalidPasswordAttemptsPeriodSeconds,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'InvalidPasswordBlockSeconds') AS InvalidPasswordBlockSeconds,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'PasswordValidity') AS PasswordValidity,
		(SELECT Value FROM ConfigurationVariables WHERE Name = 'LoginValidity') AS LoginValidity
END
GO
