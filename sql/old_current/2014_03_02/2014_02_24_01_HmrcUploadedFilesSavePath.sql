IF NOT EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'HmrcUploadedFilesSavePath')
	INSERT INTO ConfigurationVariables (Name, Value, Description)
		VALUES ('HmrcUploadedFilesSavePath', 'c:\temp\hmrc_uploaded_files', 'Absolute path to save files uploaded for HMRC accounts. Directory is created if it does not exist. Leave empty to disable file saving.')
GO
