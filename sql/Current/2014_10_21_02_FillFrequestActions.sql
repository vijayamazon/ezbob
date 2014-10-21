IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'Please send proof of ID' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('Please send proof of ID', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'We notice more than 1 mortgage in your account. What are the addresses of other properties you own?' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('We notice more than 1 mortgage in your account. What are the addresses of other properties you own?', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'We need additional PG(s): please provide DOB, living address, property address, telephone and email of the guarantor' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('We need additional PG(s): please provide DOB, living address, property address, telephone and email of the guarantor', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'Please send us the latest annual report' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('Please send us the latest annual report', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'We could not obtain a credit history at the address(-es) you provided : please send alternative address' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('We could not obtain a credit history at the address(-es) you provided : please send alternative address', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'We noticed sales decrease significantly in the last month - please explain' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('We noticed sales decrease significantly in the last month - please explain', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'We noticed sales decrease significantly in the last quarter - please explain' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('We noticed sales decrease significantly in the last quarter - please explain', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'We noticed sales decrease significantly in the last 6 months - please explain' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('We noticed sales decrease significantly in the last 6 months - please explain', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'You are not a director or owner of the company: please explain your relation to the company.' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('You are not a director or owner of the company: please explain your relation to the company.', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'Please send proof of transfer of shares and/or appoitnment as director.' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('Please send proof of transfer of shares and/or appoitnment as director.', 1)
END
GO

IF NOT EXISTS (SELECT 1 FROM FrequentActionItems WHERE Item = 'What is the purpose of the loan?' AND IsActive = 1)
BEGIN
	INSERT INTO FrequentActionItems VALUES ('What is the purpose of the loan?', 1)
END
GO
