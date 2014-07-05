IF OBJECT_ID('CustomerRelationFollowUp') IS NULL
BEGIN
	CREATE TABLE CustomerRelationFollowUp
	(
		Id INT NOT NULL IDENTITY(1,1)
	   ,CustomerId INT 
	   ,DateAdded DATETIME
	   ,FollowUpDate DATETIME
	   ,Comment NVARCHAR(1000)
	   ,CONSTRAINT PK_CustomerRelationFollowUp PRIMARY KEY (Id) 
	   ,CONSTRAINT FK_CustomerRelationFollowUp_Customer FOREIGN KEY (CustomerId) REFERENCES  Customer(Id)
	)
END  
GO

IF OBJECT_ID('CustomerRelationState') IS NULL
BEGIN
	CREATE TABLE CustomerRelationState
	(
		Id INT NOT NULL IDENTITY(1,1)
	   ,CustomerId INT 
	   ,IsFollowUp BIT 
	   ,LastFollowUpId INT 	
	   ,LastStatusId INT
	   ,LastRankId INT
	   ,CONSTRAINT PK_CustomerRelationState PRIMARY KEY (Id) 
	   ,CONSTRAINT FK_CustomerRelationState_Customer FOREIGN KEY (CustomerId) REFERENCES  Customer(Id)
	   ,CONSTRAINT FK_CustomerRelationState_CustomerRelationFollowUp FOREIGN KEY (LastFollowUpId) REFERENCES  CustomerRelationFollowUp(Id)
	   ,CONSTRAINT FK_CustomerRelationState_CRMStatuses FOREIGN KEY (LastStatusId) REFERENCES  CRMStatuses(Id)
	   ,CONSTRAINT FK_CustomerRelationState_CRMRanks FOREIGN KEY (LastRankId) REFERENCES  CRMRanks(Id)
	)
END  
GO

IF NOT EXISTS ( SELECT * FROM CRMStatuses WHERE Name = 'Need additional info') 
BEGIN
	INSERT INTO CRMStatuses (Name) VALUES ('Need additional info')
	INSERT INTO CRMStatuses (Name) VALUES ('Support request')
END
GO
