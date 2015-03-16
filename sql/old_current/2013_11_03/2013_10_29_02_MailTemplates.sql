DELETE FROM MailTemplateRelation WHERE InternalTemplateName = 'Pay early and save.docx'
GO

IF EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='LateBy14Days')
BEGIN
	DECLARE @GreetingTemplateId INT, @OldLateId INT
	SELECT @GreetingTemplateId = Id FROM MandrillTemplate WHERE NAME='Greeting'
	SELECT @OldLateId = Id FROM MandrillTemplate WHERE NAME='LateBy14Days'
	UPDATE MailTemplateRelation SET MandrillTemplateId=@GreetingTemplateId WHERE MandrillTemplateId = @OldLateId
	DELETE FROM MandrillTemplate WHERE NAME='LateBy14Days'
END
GO

UPDATE MailTemplateRelation SET InternalTemplateName = 'User is approved or re-approved by the strategy.docx' WHERE InternalTemplateName = 'User is re approved by the strategy.docx'
GO

IF EXISTS (SELECT 1 FROM MailTemplateRelation WHERE InternalTemplateName='LateChargeAddCollection.docx')
BEGIN
	DELETE FROM MailTemplateRelation WHERE InternalTemplateName='LateChargeAddCollection.docx'
	DELETE FROM MandrillTemplate WHERE NAME = 'Mandrill - Late fee was added (collection)'
END
GO

IF NOT EXISTS (SELECT 1 FROM MailTemplateRelation WHERE InternalTemplateName='Get cash - problem with bank account.docx')
BEGIN
	INSERT INTO MailTemplateRelation (InternalTemplateName, MandrillTemplateId) VALUES ('Get cash - problem with bank account.docx', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Problem with bank account')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Problem with bank account')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Get cash - problem with bank account.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - 14 days notification email')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - 14 days notification email')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Late by 14 days.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Application completed under review')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Application completed under review')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Application completed Under review.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Application incompleted AML & Bank')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Application incompleted AML & Bank')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Application incomplete - More information needed AML and Bank.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Application incompleted Bank')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Application incompleted Bank')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Application incomplete - More information needed Bank.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Automatic Re-Payment has Failed')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Automatic Re-Payment has Failed')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Automatic Re-Payment has Failed.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Confirm your email')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Confirm your email')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='EmailConfirmation.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Debit card authorization problem')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Debit card authorization problem')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Get cash - problem with the card.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - EZBOB password was restored')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - EZBOB password was restored')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='RestorePassword.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Late fee was added (14D late)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Late fee was added (14D late)')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='LateChargeAddSecond.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Late fee was added (7D late)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Late fee was added (7D late)')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='LateChargeAdd.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Loan paid in full')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Loan paid in full')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='thanks for paying in full and benefits for next loan.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Loan repayment late (1D late)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Loan repayment late (1D late)')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='latepayment.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - New password')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - New password')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='You changed your password. Your new password is.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Partial repayment')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Partial repayment')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='partialpayment.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Payment reminder')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Payment reminder')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='paymentreminder.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - PayPoint data differs')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - PayPoint data differs')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='PayPointNameValidationFailed.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Re-analyzing customer')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Re-analyzing customer')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Didnt take offer and re-applies.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Rejection email')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Rejection email')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Unfortunately you have been unable to qualify.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Renew your eBay token')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Renew your eBay token')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='ReneweBayToken.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Repayment confirmation')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Repayment confirmation')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='EZBOB payment early repayment confirmation.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Rollover added')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Rollover added')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='RolloverPaid.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Temporary password')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Temporary password')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Sending temporary password after three failed login attempts.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Application incompleted AML')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Application incompleted AML')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Application incomplete - More information needed AML.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - CAIS report')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - CAIS report')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='CAIS report.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Collection')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Collection')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Collection.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - DS Exception without umi')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - DS Exception without umi')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='DS Exception without umi.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - email to admin')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - email to admin')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='email to admin.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - No Information about shops')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - No Information about shops')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='No Information about shops.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - PayPoint Script Exception')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - PayPoint Script Exception')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='PayPoint Script Exception.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Underwriter added a debit card')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Underwriter added a debit card')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Underwriter added a debit card.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Update MP Error Code')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Update MP Error Code')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Update MP Error Code.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - UpdateCMP Error')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - UpdateCMP Error')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='UpdateCMP Error.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - User is approved or re-approved')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - User is approved or re-approved')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='User is approved or re-approved by the strategy.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - User is rejected by the strategy')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - User is rejected by the strategy')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='User is rejected by the strategy.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - User is waiting for decision')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - User is waiting for decision')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='User is waiting for decision.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - User was escalated')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - User was escalated')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='User was escalated.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Approval (not 1st time)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Approval (not 1st time)')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Congratulations you are qualified - not first.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Approval (1st time)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Approval (1st time)')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Congratulations you are qualified.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Approval Offline (not 1st time)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Approval Offline (not 1st time)')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Congratulations you are qualified - offline - not first.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Approval Offline (1st time)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Approval Offline (1st time)')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Congratulations you are qualified - offline.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Partial repayment')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Partial repayment')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='fullrepayment.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Took Loan (not 1st loan)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Took Loan (not 1st loan)')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Get cash - approval - not first.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Took Loan (1st loan)')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Took Loan (1st loan)')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Get cash - approval.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - 2 days notice')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - 2 days notice')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='Your account will be billed to schedule.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - 5 days notice')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - 5 days notice')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='your payment is due in 5 days.docx'
END
GO

IF NOT EXISTS (SELECT 1 FROM MandrillTemplate WHERE NAME='Mandrill - Received Full Repayment')
BEGIN
	INSERT INTO MandrillTemplate (NAME) VALUES ('Mandrill - Received Full Repayment')
	UPDATE MailTemplateRelation SET MandrillTemplateId = @@IDENTITY WHERE InternalTemplateName='fullrepayment.docx'
END
GO















