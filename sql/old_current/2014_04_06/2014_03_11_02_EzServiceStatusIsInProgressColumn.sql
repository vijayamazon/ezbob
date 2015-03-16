IF NOT EXISTS(SELECT * FROM sys.columns where Name = N'IsInProgress' and Object_ID = Object_ID(N'EzServiceActionStatus'))
BEGIN 
	ALTER TABLE EzServiceActionStatus ADD IsInProgress BIT
END 	

GO
UPDATE EzServiceActionStatus SET IsInProgress = 0
UPDATE EzServiceActionStatus SET IsInProgress = 1 WHERE ActionStatusName='In progress'

GO

