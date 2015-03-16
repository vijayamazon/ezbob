IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE Name='CompanyFilesSavePath')
BEGIN 
INSERT INTO dbo.ConfigurationVariables(Name, Value, Description)
 VALUES('CompanyFilesSavePath', 'c:\temp\company_files', 'Absolute path to save files uploaded for company files. Directory is created if it does not exist. Leave empty to disable file saving.')
END 
GO
