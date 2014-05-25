IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'UnderwriterId' and Object_ID = Object_ID(N'ReportUsers'))    
BEGIN

ALTER TABLE ReportUsers ADD UnderwriterId INT
ALTER TABLE ReportUsers ADD CONSTRAINT FK_ReportUsers_Security_User FOREIGN KEY (UnderwriterId) REFERENCES Security_User(UserId)
		
END 

GO
