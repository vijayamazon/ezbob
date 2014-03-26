IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmlCacheValidForSeconds')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('AmlCacheValidForSeconds', 3888000, 'Number of seconds for which the AML cache is valid for')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BwaCacheValidForSeconds')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('BwaCacheValidForSeconds', 3888000, 'Number of seconds for which the BWA cache is valid for')
END
GO
