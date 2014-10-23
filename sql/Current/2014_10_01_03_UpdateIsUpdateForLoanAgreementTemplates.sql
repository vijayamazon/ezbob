IF NOT EXISTS (SELECT 1 FROM LoanAgreementTemplate WHERE IsUpdate = 1)
BEGIN
	DECLARE @TemplateId INT

	SELECT @TemplateId = MAX(Id) FROM LoanAgreementTemplate WHERE TemplateType = 1
	UPDATE LoanAgreementTemplate SET IsUpdate = 1 WHERE Id = @TemplateId 

	SELECT @TemplateId = MAX(Id) FROM LoanAgreementTemplate WHERE TemplateType = 2
	UPDATE LoanAgreementTemplate SET IsUpdate = 1 WHERE Id = @TemplateId 

	SELECT @TemplateId = MAX(Id) FROM LoanAgreementTemplate WHERE TemplateType = 3
	UPDATE LoanAgreementTemplate SET IsUpdate = 1 WHERE Id = @TemplateId 

	SELECT @TemplateId = MAX(Id) FROM LoanAgreementTemplate WHERE TemplateType = 4
	UPDATE LoanAgreementTemplate SET IsUpdate = 1 WHERE Id = @TemplateId 
END
GO
