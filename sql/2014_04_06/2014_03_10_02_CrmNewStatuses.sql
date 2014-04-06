IF NOT EXISTS (SELECT * FROM CRMStatuses WHERE Name='Default letter sent') 
BEGIN
	INSERT INTO CRMStatuses (Name) VALUES ('Default letter sent')
	INSERT INTO CRMStatuses (Name) VALUES ('Arrears letter sent')
	INSERT INTO CRMStatuses (Name) VALUES ('Termination letter sent')
END 