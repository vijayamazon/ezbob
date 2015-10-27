IF object_id('CollectionSmsTemplate') IS NULL
BEGIN
CREATE TABLE CollectionSmsTemplate(
	CollectionSmsTemplateID INT NOT NULL IDENTITY(1,1),
	Type NVARCHAR(30),	
	IsActive BIT NOT NULL,
	OriginID INT NOT NULL,
	Template NVARCHAR(500),
	Comment NVARCHAR(500),
	CONSTRAINT PK_CollectionSmsTemplate PRIMARY KEY (CollectionSmsTemplateID),
	CONSTRAINT FK_CollectionSmsTemplate_Origin FOREIGN KEY (OriginID) REFERENCES CustomerOrigin(CustomerOriginID)
)	

INSERT INTO CollectionSmsTemplate (Type, IsActive, OriginID, Template, Comment) VALUES
	('CollectionDay0', 1, 1, '{0} This is to remind you that payment of {1} is overdue with ezbob. Please call the collections team on 02036677519 ASAP.','{0} - FirstName, {1} - Amount Due'),
	('CollectionDay0', 1, 2, '{0} This is to remind you that payment of {1} is overdue with Everline. Please call the collections team on 02036677519 ASAP.','{0} - FirstName, {1} - Amount Due'),
	('CollectionDay1to6', 1, 1, '{0} Your outstanding balance of {1} with ezbob has not been settled. To avoid additional late payment fees call collections on 02036677519','{0} - FirstName, {1} - Amount Due'),
	('CollectionDay1to6', 1, 2, '{0} Your outstanding balance of {1} with Everline has not been settled. To avoid additional late payment fees call collections on 02036677519','{0} - FirstName, {1} - Amount Due'),
	('CollectionDay7', 1, 1, '{0}, the {1}, balance including £20 late payment on your ezbob account is overdue. To avoid debt collection proceedings call 02036677519','{0} - FirstName, {1} - Amount Due'),
	('CollectionDay7', 1, 2, '{0}, the {1}, balance including £20 late payment on your Everline account is overdue. To avoid debt collection proceedings call 02036677519','{0} - FirstName, {1} - Amount Due'),
	('CollectionDay8to14', 1, 1, 'Warning - {0} late fees and daily interest have been added to your account. Act now to avoid further action call collections on 02036677519','{0} - FirstName'),
	('CollectionDay8to14', 1, 2, 'Warning - {0} late fees and daily interest have been added to your account. Act now to avoid further action call collections on 02036677519','{0} - FirstName'),
	('CollectionDay15', 1, 1, '{0} Final reminder: Payment of {1} was due on {2}. If we do not receive payment in full we will submit your account to our legal department.','{0} - FirstName, {1} - Amount Due, {2} - Due Date'),
	('CollectionDay15', 1, 2, '{0} Final reminder: Payment of {1} was due on {2}. If we do not receive payment in full we will submit your account to our legal department.','{0} - FirstName, {1} - Amount Due, {2} - Due Date'),
	('CollectionDay21', 1, 1, '{0} you have failed to settle your payment. ezbob has submitted your account for legal proceedings. Please call collections on 02036677519','{0} - FirstName'),
	('CollectionDay21', 1, 2, '{0} you have failed to settle your payment. Everline has submitted your account for legal proceedings. Please call collections on 02036677519','{0} - FirstName'),
	('CollectionDay31', 1, 1, '{0} You have failed to settle your payment. ezbob has submitted your account for legal proceedings. Call collections on 02036677519','{0} - FirstName'),
	('CollectionDay31', 1, 2, '{0} You have failed to settle your payment. Everline has submitted your account for legal proceedings. Call collections on 02036677519','{0} - FirstName')
END
GO