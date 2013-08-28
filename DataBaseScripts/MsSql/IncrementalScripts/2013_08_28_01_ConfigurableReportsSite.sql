IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='ReportsSite')
BEGIN
	INSERT INTO ConfigurationVariables VALUES ('ReportsSite', 'http://192.168.120.6:81/Login.aspx', 'ip address of reports site')
END
GO
