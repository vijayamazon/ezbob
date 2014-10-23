IF NOT EXISTS (SELECT 1 FROM CRMActions WHERE Name='Action items change')
BEGIN
	INSERT INTO CRMActions(Name) VALUES ('Action items change')
END
GO
