-------------------------------------------------------------------------------
--
-- Create IsBroker column in relevant tables
--
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsBroker' AND id = OBJECT_ID('CustomerRelations'))
	ALTER TABLE CustomerRelations ADD IsBroker BIT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsBroker' AND id = OBJECT_ID('CustomerRelationFollowUp'))
	ALTER TABLE CustomerRelationFollowUp ADD IsBroker BIT NULL
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE name = 'IsBroker' AND id = OBJECT_ID('CRMStatusGroup'))
	ALTER TABLE CRMStatusGroup ADD IsBroker BIT NULL
GO

-------------------------------------------------------------------------------
--
-- Fill IsBroker column
--
-------------------------------------------------------------------------------

UPDATE CustomerRelations SET IsBroker = 0 WHERE ISNULL(IsBroker, 0) != 1
GO

UPDATE CustomerRelationFollowUp SET IsBroker = 0 WHERE ISNULL(IsBroker, 0) != 1
GO

UPDATE CRMStatusGroup SET IsBroker = 0 WHERE ISNULL(IsBroker, 0) != 1
GO

-------------------------------------------------------------------------------
--
-- Create Broker group
--
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM CRMStatusGroup WHERE Name = 'Broker')
	INSERT INTO CRMStatusGroup (Name, Priority, IsBroker) VALUES ('Broker', 1, 1)
GO

-------------------------------------------------------------------------------
--
-- Create Broker statuses
--
-------------------------------------------------------------------------------

DECLARE @BrokerGrpID INT

-------------------------------------------------------------------------------

SELECT
	@BrokerGrpID = Id
FROM
	CRMStatusGroup
WHERE
	Name = 'Broker'
	AND
	IsBroker = 1

-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT * FROM CRMStatuses WHERE Name LIKE 'No answer%' AND GroupId = @BrokerGrpID)
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('No answer (broker)', @BrokerGrpID)

IF NOT EXISTS (SELECT * FROM CRMStatuses WHERE Name LIKE 'Pending%' AND GroupId = @BrokerGrpID)
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Pending (broker)', @BrokerGrpID)
GO

