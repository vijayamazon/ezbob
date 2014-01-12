UPDATE
	Customer
SET
	LimitedCompanyNumber = rtrim(ltrim(LimitedCompanyNumber))
WHERE 
	LimitedCompanyNumber != rtrim(ltrim(LimitedCompanyNumber))
GO
