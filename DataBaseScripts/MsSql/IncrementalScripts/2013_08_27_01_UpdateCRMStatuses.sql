IF EXISTS (SELECT 1 FROM CustomerRelations WHERE StatusId IN (SELECT Id FROM CRMStatuses WHERE Name='InProcess'))
BEGIN
	DECLARE @NoSaleStatus INT
	SELECT @NoSaleStatus = Id FROM CRMStatuses WHERE Name='NoSale'
	UPDATE CustomerRelations SET StatusId = @NoSaleStatus WHERE StatusId IN (SELECT Id FROM CRMStatuses WHERE Name='InProcess')
END

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Legal')
	INSERT INTO CRMStatuses VALUES ('Legal')
	
IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Pending')
	INSERT INTO CRMStatuses VALUES ('Pending')

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Collection')
	INSERT INTO CRMStatuses VALUES ('Collection')

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'P Feedback')
	INSERT INTO CRMStatuses VALUES ('P Feedback')

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'N Feedback')
	INSERT INTO CRMStatuses VALUES ('N Feedback')

GO
