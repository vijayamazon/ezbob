DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL OR @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianCertificateThumb')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianCertificateThumb', 'a3fe6b5cc7658f2aff29882a3623772f9502df55', 'Experian certificate thumb')
	END	
END
ELSE
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianCertificateThumb')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianCertificateThumb', 'ac4c796127a2b6c3dc30ab005e8444201cffd454', 'Experian certificate thumb')
	END	
END
GO
