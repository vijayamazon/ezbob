IF OBJECT_ID('GetUserManagementConfiguration') IS NULL
	EXECUTE('CREATE PROCEDURE GetUserManagementConfiguration AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE GetUserManagementConfiguration
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Name,
		Value
	FROM
		ConfigurationVariables
	WHERE
		Name IN (
			'LoginValidationStringForWeb',
			'NumOfInvalidPasswordAttempts',
			'InvalidPasswordAttemptsPeriodSeconds',
			'InvalidPasswordBlockSeconds',
			'PasswordValidity',
			'LoginValidity'
		)
	UNION
	SELECT
		'__UnderwriterLogin__',
		UserName
	FROM
		Security_User
	WHERE
		BranchId = 1
END
GO
