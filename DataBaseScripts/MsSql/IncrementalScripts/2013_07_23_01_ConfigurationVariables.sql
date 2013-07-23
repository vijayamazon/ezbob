-- a
IF ( SELECT COUNT(*) FROM ConfigurationVariables cv WHERE NAME = 'FinancialAccounts_MainApplicant' ) = 0 BEGIN
	INSERT INTO ConfigurationVariables(	Name, [Value],[Description])VALUES('FinancialAccounts_MainApplicant', 0, 'if enabled this type of financial accounts will be shown in corresponding area on Credit Bureau tab ')
END
-- b
IF ( SELECT COUNT(*) FROM ConfigurationVariables cv WHERE NAME = 'FinancialAccounts_AliasOfMainApplicant' ) = 0 BEGIN
	INSERT INTO ConfigurationVariables(	Name, [Value],[Description])VALUES('FinancialAccounts_AliasOfMainApplicant', 0, 'if enabled this type of financial accounts will be shown in corresponding area on Credit Bureau tab ')
END
-- c
IF ( SELECT COUNT(*) FROM ConfigurationVariables cv WHERE NAME = 'FinancialAccounts_AssociationOfMainApplicant' ) = 0 BEGIN
	INSERT INTO ConfigurationVariables(	Name, [Value],[Description])VALUES('FinancialAccounts_AssociationOfMainApplicant', 0, 'if enabled this type of financial accounts will be shown in corresponding area on Credit Bureau tab ')
END
-- d
IF ( SELECT COUNT(*) FROM ConfigurationVariables cv WHERE NAME = 'FinancialAccounts_JointApplicant' ) = 0 BEGIN
	INSERT INTO ConfigurationVariables(	Name, [Value],[Description])VALUES('FinancialAccounts_JointApplicant', 0, 'if enabled this type of financial accounts will be shown in corresponding area on Credit Bureau tab ')
END
-- e
IF ( SELECT COUNT(*) FROM ConfigurationVariables cv WHERE NAME = 'FinancialAccounts_AliasOfJointApplicant' ) = 0 BEGIN
	INSERT INTO ConfigurationVariables(	Name, [Value],[Description])VALUES('FinancialAccounts_AliasOfJointApplicant', 0, 'if enabled this type of financial accounts will be shown in corresponding area on Credit Bureau tab ')
END
-- f
IF ( SELECT COUNT(*) FROM ConfigurationVariables cv WHERE NAME = 'FinancialAccounts_AssociationOfJointApplicant' ) = 0 BEGIN
	INSERT INTO ConfigurationVariables(	Name, [Value],[Description])VALUES('FinancialAccounts_AssociationOfJointApplicant', 0, 'if enabled this type of financial accounts will be shown in corresponding area on Credit Bureau tab ')
END
-- g
IF ( SELECT COUNT(*) FROM ConfigurationVariables cv WHERE NAME = 'FinancialAccounts_No_Match' ) = 0 BEGIN
	INSERT INTO ConfigurationVariables(	Name, [Value],[Description])VALUES('FinancialAccounts_No_Match', 0, 'if enabled this type of financial accounts will be shown in corresponding area on Credit Bureau tab')
END