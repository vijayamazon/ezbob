IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'TotalMonthlySalary' and Object_ID = Object_ID(N'CompanyEmployeeCount'))    
BEGIN
	ALTER TABLE CompanyEmployeeCount ADD TotalMonthlySalary DECIMAL(18) NOT NULL DEFAULT(0)
END 