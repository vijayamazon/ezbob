IF NOT EXISTS (SELECT 1 FROM ConfigurationVariables WHERE NAME = 'Reject_Minimal_Seniority')
BEGIN
	INSERT INTO ConfigurationVariables(	Name, [Value],[Description])VALUES('Reject_Minimal_Seniority', 300, 'The number is in days. This is threshold for auto rejection process')
END
GO
