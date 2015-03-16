IF OBJECT_ID('FK_CustomerRelationFollowUp_Customer') IS NOT NULL
	ALTER TABLE CustomerRelationFollowUp DROP CONSTRAINT FK_CustomerRelationFollowUp_Customer
GO

IF OBJECT_ID('FK_CustomerRelationFollowUp_User') IS NULL
	ALTER TABLE CustomerRelationFollowUp ADD CONSTRAINT FK_CustomerRelationFollowUp_User FOREIGN KEY (CustomerId) REFERENCES Security_User (UserId)
GO
