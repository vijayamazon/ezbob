IF NOT EXISTS (SELECT * FROM CRMActions WHERE Name = 'SMS') INSERT INTO CRMActions (Name) VALUES ('SMS')
GO