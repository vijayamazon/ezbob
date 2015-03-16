IF NOT EXISTS(SELECT * FROM syscolumns WHERE id = OBJECT_ID('PostcodeServiceLog') AND name = 'UserID')
BEGIN
	ALTER TABLE PostcodeServiceLog ADD UserID INT NULL

	ALTER TABLE PostcodeServiceLog ADD CONSTRAINT FK_PostcodeServiceLog_User FOREIGN KEY (UserID) REFERENCES Security_User(UserId)
END
GO
