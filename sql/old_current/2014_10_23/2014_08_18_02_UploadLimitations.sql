UPDATE UploadLimitations SET
	AcceptedFiles = 'application/pdf'
WHERE
	ControllerName IN ('HmrcController', 'UploadHmrcController')
	AND
	ActionName = 'SaveFile'
GO
