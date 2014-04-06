IF EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='AmlCacheValidForSeconds')
BEGIN
	DELETE FROM ConfigurationVariables WHERE Name='AmlCacheValidForSeconds'
END
GO

IF EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='BwaCacheValidForSeconds')
BEGIN
	DELETE FROM ConfigurationVariables WHERE Name='BwaCacheValidForSeconds'
END
GO
