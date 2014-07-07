UPDATE CRMStatusGroup SET Priority = 3 WHERE Name = 'Collection'
GO

UPDATE CRMStatusGroup SET Priority = 4 WHERE Name = 'Other'
GO

IF NOT EXISTS (SELECT 1 FROM CRMStatusGroup WHERE Name = 'Underwriters')
BEGIN
	INSERT INTO CRMStatusGroup (Name, Priority) VALUES ('Underwriters', 2)
END 
GO

DECLARE @SalesGrp INT, @CollectionGrp INT, @UnderwritersGrp INT, @OtherGrp INT

SELECT @SalesGrp = Id FROM CRMStatusGroup WHERE Name = 'Sales'
SELECT @CollectionGrp = Id FROM CRMStatusGroup WHERE Name = 'Collection'
SELECT @UnderwritersGrp = Id FROM CRMStatusGroup WHERE Name = 'Underwriters'
SELECT @OtherGrp = Id FROM CRMStatusGroup WHERE Name = 'Other'


IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'No Answer')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('No Answer', @SalesGrp)
END 

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Info Request Sent')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Info Request Sent', @SalesGrp)
END 

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Info received')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Info received', @SalesGrp)
END 

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Offer ready')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Offer ready', @SalesGrp)
END 

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Offer Sent')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Offer Sent', @SalesGrp)
END 

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Contracts Signed')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Contracts Signed', @SalesGrp)
END 


IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Note for underwriting')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Note for underwriting', @UnderwritersGrp)
END


IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Default letter sent')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Default letter sent', @CollectionGrp)
END 

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Arrears letter sent')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Arrears letter sent', @CollectionGrp)
END 

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Termination letter sent')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Termination letter sent', @CollectionGrp)
END 


IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Need additional info')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Need additional info', @OtherGrp)
END 

IF NOT EXISTS (SELECT 1 FROM CRMStatuses WHERE Name = 'Support request')
BEGIN
	INSERT INTO CRMStatuses (Name, GroupId) VALUES ('Support request', @OtherGrp)
END 

GO
