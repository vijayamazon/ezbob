DECLARE @Environment NVARCHAR(256)
SELECT @Environment = Value FROM ConfigurationVariables WHERE Name = 'Environment'

IF @Environment = 'Dev' OR @Environment IS NULL OR @Environment = 'QA' OR @Environment = 'UAT'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianCertificateThumb')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianCertificateThumb', '9F63413AE1EE1756270B4D4FEE276249ADB8BB83', 'Experian certificate thumb')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianInteractiveService')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianInteractiveService', 'https://scems.uat.uk.experian.com/experian/wbsv/v100/interactive.asmx', 'Experian interactive service')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianAuthTokenService')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianAuthTokenService', 'https://secure.wasp.uat.uk.experian.com/WASPAuthenticator/tokenService.asmx', 'Experian auth token service')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianAuthTokenServiceIdHub')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianAuthTokenServiceIdHub', 'https://secure.wasp.uat.uk.experian.com/WASPAuthenticator/tokenService.asmx', 'Experian auth token service id hub')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianIdHubService')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianIdHubService', 'https://ukid.uat.uk.experian.com/EIHEndpoint', 'Experian id hub service')
	END
	
	IF @Environment = 'Dev' OR @Environment IS NULL
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianUIdCertificateThumb')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianUIdCertificateThumb', 'F6D0B8CBB6F77B390C368B7B481352FBFAC31D34', 'Experian UIdCertificateThumb')
		END
	END
	ELSE
	BEGIN
		IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianUIdCertificateThumb')
		BEGIN
			INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianUIdCertificateThumb', '57bab649510e75988b7f5483c674f4488c4b2c16', 'Experian UIdCertificateThumb')
		END
	END
END

IF @Environment = 'Prod'
BEGIN
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianCertificateThumb')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianCertificateThumb', 'EF1974035B53A40852B190C523D2A9D3C67BE812', 'Experian certificate thumb')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianInteractiveService')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianInteractiveService', 'https://scems.uk.experian.com/experian/wbsv/v100/interactive/interactive.asmx', 'Experian interactive service')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianAuthTokenService')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianAuthTokenService', 'https://secure.wasp.uk.experian.com/WASPAuthenticator/tokenService.asmx', 'Experian auth token service')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianAuthTokenServiceIdHub')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianAuthTokenServiceIdHub', 'https://secure.authenticator.uk.experian.com/WASPAuthenticator/TokenService.asmx', 'Experian auth token service id hub')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianIdHubService')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianIdHubService', 'https://ukid.uk.experian.com/EIHEndpoint', 'Experian id hub service')
	END
	IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianUIdCertificateThumb')
	BEGIN
		INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianUIdCertificateThumb', '9f329e2eb46454edad175e8f8bb08b20b385c5d7', 'Experian UIdCertificateThumb')
	END
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianESeriesUrl')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianESeriesUrl', 'http://192.168.100.2:8888/e-series/Controller', 'Experian e-series url')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianInteractiveMode')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianInteractiveMode', 'Oneshot', 'Experian interactive mode')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianSecureFtpHostName')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianSecureFtpHostName', 'https://secureftp.experian.co.uk/Upload', 'Experian secure ftp host name')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianSecureFtpUserName')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianSecureFtpUserName', 'PIF1160_AyCpjxzt25', 'Experian secure ftp user name')
END
GO

IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ExperianSecureFtpUserPassword')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('ExperianSecureFtpUserPassword', 'dWiPyfo5>V', 'Experian secure ftp user password')
END
GO
