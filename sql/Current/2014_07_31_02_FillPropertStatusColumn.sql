IF NOT EXISTS (SELECT 1 FROM Customer WHERE PropertyStatusId IS NOT NULL)
BEGIN
	UPDATE Customer SET PropertyStatusId = 2 WHERE ResidentialStatus = 'Home owner'
	UPDATE Customer SET PropertyStatusId = 5 WHERE ResidentialStatus = 'Renting'
	UPDATE Customer SET PropertyStatusId = 7 WHERE ResidentialStatus = 'Living with Parents'
END
GO

