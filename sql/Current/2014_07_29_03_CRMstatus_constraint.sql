IF OBJECT_ID('FK_CustomerRelationState_Customer') IS NOT NULL
	ALTER TABLE CustomerRelationState DROP CONSTRAINT FK_CustomerRelationState_Customer
GO

IF OBJECT_ID('FK_CustomerRelationState_User') IS NULL
	ALTER TABLE CustomerRelationState ADD CONSTRAINT FK_CustomerRelationState_User FOREIGN KEY (CustomerId) REFERENCES Security_User(UserId)
GO
