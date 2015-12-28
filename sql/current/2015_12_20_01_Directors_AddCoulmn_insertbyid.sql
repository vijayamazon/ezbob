IF NOT EXISTS (SELECT * FROM syscolumns WHERE name='UserId' AND id=object_id('Director'))
BEGIN
	ALTER TABLE Director
	    ADD UserId INTEGER,
	    FOREIGN KEY(UserId) REFERENCES Security_User(UserId)
END
GO
