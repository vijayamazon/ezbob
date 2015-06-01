
IF NOT EXISTS (SELECT * FROM CRMStatuses WHERE Name ='Payment' and GroupId=2)
BEGIN
	INSERT INTO CRMStatuses (Name,GroupId) VALUES ('Payment',2)
END 
GO


IF NOT EXISTS (SELECT * FROM CRMStatuses WHERE Name ='Negotiate' and GroupId=2)
BEGIN
	INSERT INTO CRMStatuses (Name,GroupId) VALUES ('Negotiate',2)
END 
GO


IF NOT EXISTS (SELECT * FROM CRMStatuses WHERE Name ='Arrangement' and GroupId=2)
BEGIN
	INSERT INTO CRMStatuses (Name,GroupId) VALUES ('Arrangement',2)
END 
GO


IF NOT EXISTS (SELECT * FROM CRMStatuses WHERE Name ='Rollover' and GroupId=2)
BEGIN
	INSERT INTO CRMStatuses (Name,GroupId) VALUES ('Rollover',2)
END 
GO

