IF NOT EXISTS (SELECT * FROM CRMStatuses WHERE Name='Info Request Sent')
BEGIN
	DECLARE @Sales INT = (SELECT Id FROM CRMStatusGroup WHERE Name='Sales')
	INSERT INTO dbo.CRMStatuses(Name, GroupId) VALUES ('Info Request Sent', @Sales)
	INSERT INTO dbo.CRMStatuses(Name, GroupId) VALUES ('Info received', @Sales)
	INSERT INTO dbo.CRMStatuses(Name, GroupId) VALUES ('Offer ready', @Sales)
	INSERT INTO dbo.CRMStatuses(Name, GroupId) VALUES ('Offer Sent', @Sales)
	INSERT INTO dbo.CRMStatuses(Name, GroupId) VALUES ('Contracts Signed', @Sales)
END 
GO