DELETE FROM MailTemplateRelation
go

--remove all pdf documents
DELETE FROM Export_Results WHERE FileType = 1
GO

ALTER TABLE MailTemplateRelation DROP CONSTRAINT UNIQUE_InternalTemplateName
GO
                  
ALTER TABLE MailTemplateRelation 
ADD CONSTRAINT UNIQUE_InternalTemplateName UNIQUE (InternalTemplateName);
GO

INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Application completed Under review.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Application incomplete - More information needed AML and Bank.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Application incomplete - More information needed AML.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Application incomplete - More information needed Bank.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Automatic Re-Payment has Failed.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'CAIS report.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Collection.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Congratulations you are qualified.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Didnt take offer and re-applies.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'DS Exception without umi.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'email to admin.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'EmailConfirmation.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'EZBOB payment early repayment confirmation.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'fullrepayment.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Get cash - approval.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Get cash - problem with the card.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'LateChargeAdd.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'LateChargeAddCollection.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'LateChargeAddSecond.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'latepayment.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'No Information about shops.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'partialpayment.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Pay early and save.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'paymentreminder.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'PayPoint Script Exception.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'PayPointNameValidationFailed.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'ReneweBayToken.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'RestorePassword.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'RolloverPaid.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Sending temporary password after three failed login attempts.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Thanks for joining us.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'thanks for paying in full and benefits for next loan.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Underwriter added a debit card.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Unfortunately you have been unable to qualify.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Update MP Error Code.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'UpdateCMP Error.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'User is re approved by the strategy.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'User is rejected by the strategy.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'User is waiting for decision.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'User was escalated.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'You changed your password. Your new password is.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'Your account will be billed to schedule.docx', 1)
INSERT INTO MailTemplateRelation(InternalTemplateName,MandrillTemplateId)VALUES( 'your payment is due in 5 days.docx', 1)