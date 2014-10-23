UPDATE UploadLimitations SET
	AcceptedFiles = 'image/*,application/pdf,.doc,.docx,.odt,.rtf,.ppt,.pptx,.odp,.xls,.xlsx,.ods,.txt,.csv,.xml,.htm,.html,.xhtml,.mht,.msg,.eml'
WHERE
	ControllerName IS NULL
GO

UPDATE UploadLimitations SET
	FileSizeLimit = 10000000
WHERE
	ControllerName = 'CompanyFilesMarketPlaces'
	AND
	ActionName = 'UploadedFiles'
GO
