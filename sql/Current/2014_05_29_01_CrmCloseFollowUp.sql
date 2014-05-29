IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'IsClosed' and Object_ID = Object_ID(N'CustomerRelationFollowUp'))    
BEGIN

ALTER TABLE CustomerRelationFollowUp ADD IsClosed BIT DEFAULT(0) NOT NULL 
ALTER TABLE CustomerRelationFollowUp ADD CloseDate DATETIME
		
END 
GO