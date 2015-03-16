IF OBJECT_ID (N'dbo.Company') IS NULL
BEGIN

CREATE TABLE dbo.Company
	(
	  Id                     INT IDENTITY NOT NULL
	, CustomerId             INT NOT NULL
	, TypeOfBusiness         NVARCHAR (50)
	, CompanyNumber          NVARCHAR (100)
	, CompanyName            NVARCHAR (300)
	, TimeAtAddress          INT
	, TimeInBusiness         NVARCHAR (250)
	, BusinessPhone          NVARCHAR (50)
	, PropertyOwnedByCompany BIT
	, YearsInCompany         NVARCHAR (50)
	, RentMonthLeft          NVARCHAR (50)
	, CapitalExpenditure     DECIMAL (18) DEFAULT ((0))
	, ExperianRefNum         NVARCHAR (50)
	, ExperianCompanyName    NVARCHAR (300)
	, CONSTRAINT PK_Company PRIMARY KEY (Id)
	, CONSTRAINT FK_Company_Customer FOREIGN KEY (CustomerId) REFERENCES dbo.Customer (Id)
	)
	
	ALTER TABLE Director ADD CompanyId INT 
	ALTER TABLE Director ADD CONSTRAINT FK_Director_Company FOREIGN KEY (CompanyId) REFERENCES Company(Id)
	
	ALTER TABLE CustomerAddress ADD CompanyId INT 
	ALTER TABLE CustomerAddress ADD CONSTRAINT FK_CustomerAddress_Company FOREIGN KEY (CompanyId) REFERENCES Company(Id)

	ALTER TABLE CompanyEmployeeCount ADD CompanyId INT 
	ALTER TABLE CompanyEmployeeCount ADD CONSTRAINT FK_CompanyEmployeeCount_Company FOREIGN KEY (CompanyId) REFERENCES Company(Id)
	
	INSERT INTO Company (CustomerId 
	, TypeOfBusiness    
	, CompanyNumber     
	, CompanyName       
	, TimeAtAddress     
	, TimeInBusiness    
	, BusinessPhone     
	, PropertyOwnedByCompany 
	, YearsInCompany         
	, RentMonthLeft          
	, CapitalExpenditure     
	, ExperianRefNum)
	SELECT Id AS CustomerId, TypeOfBusiness, LimitedCompanyNumber CompanyNumber, LimitedCompanyName CompanyName, LimitedTimeAtAddress TimeAtAddress, NULL AS TimeInBusiness, LimitedBusinessPhone, PropertyOwnedByCompany, YearsInCompany, RentMonthsLeft, CapitalExpenditure, LimitedRefNum AS ExperianRefNum 
	FROM Customer 
	WHERE TypeOfBusiness IN ('LLP', 'Limited')
	
	INSERT INTO Company (CustomerId 
	, TypeOfBusiness    
	, CompanyNumber     
	, CompanyName       
	, TimeAtAddress     
	, TimeInBusiness    
	, BusinessPhone     
	, PropertyOwnedByCompany 
	, YearsInCompany         
	, RentMonthLeft          
	, CapitalExpenditure     
	, ExperianRefNum)
	SELECT Id AS CustomerId, TypeOfBusiness, NULL AS CompanyNumber, NonLimitedCompanyName CompanyName, NonLimitedTimeAtAddress TimeAtAddress, NonLimitedTimeInBusiness AS TimeInBusiness, NonLimitedBusinessPhone, PropertyOwnedByCompany, YearsInCompany, RentMonthsLeft, CapitalExpenditure, NonLimitedRefNum AS ExperianRefNum 
	FROM Customer 
	WHERE TypeOfBusiness IN ('PShip3P', 'SoleTrader', 'PShip')

	UPDATE Director SET Director.CompanyId = Company.Id
	FROM Director, Company 
	WHERE Director.CustomerId = Company.CustomerId
	
	UPDATE CustomerAddress SET CustomerAddress.CompanyId = Company.Id
	FROM CustomerAddress, Company 
	WHERE CustomerAddress.CustomerId = Company.CustomerId 
	AND CustomerAddress.addressType>2 AND CustomerAddress.addressType<11
	
	UPDATE CompanyEmployeeCount SET CompanyEmployeeCount.CompanyId = Company.Id
	FROM CompanyEmployeeCount, Company 
	WHERE CompanyEmployeeCount.CustomerId = Company.CustomerId
END
GO


