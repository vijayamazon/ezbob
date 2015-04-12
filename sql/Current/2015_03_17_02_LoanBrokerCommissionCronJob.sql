IF NOT EXISTS (SELECT * FROM EzServiceActionName WHERE ActionName = 'Ezbob.Backend.Strategies.Broker.BrokerTransferCommission')
BEGIN 
	INSERT INTO dbo.EzServiceActionName (ActionName)
	VALUES	('Ezbob.Backend.Strategies.Broker.BrokerTransferCommission')
END	
GO

DECLARE @BrokerTransferCommissionActionNameID INT = (SELECT ActionNameID FROM EzServiceActionName WHERE ActionName = 'Ezbob.Backend.Strategies.Broker.BrokerTransferCommission')

IF NOT EXISTS (SELECT * FROM EzServiceCrontab WHERE ActionNameID = @BrokerTransferCommissionActionNameID) 
BEGIN
	INSERT INTO EzServiceCrontab
		(ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
	VALUES
		(@BrokerTransferCommissionActionNameID, 0, 2, '2014-01-01 10:45:00')
		
	INSERT INTO EzServiceCrontab
		(ActionNameID, IsEnabled, RepetitionTypeID, RepetitionTime)
	VALUES
		(@BrokerTransferCommissionActionNameID, 0, 2, '2014-01-01 20:45:00')
END 		
GO
