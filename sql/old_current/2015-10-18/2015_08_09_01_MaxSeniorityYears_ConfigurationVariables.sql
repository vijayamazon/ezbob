IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='MaxSeniorityYears')
BEGIN
	INSERT INTO ConfigurationVariables(Name, Value, Description) VALUES ('MaxSeniorityYears', '50', 'Company Seniority to set fraud alert from');
END
ELSE
BEGIN
	UPDATE ConfigurationVariables SET  Value = '50' WHERE Name = 'MaxSeniorityYears';
END
GO
