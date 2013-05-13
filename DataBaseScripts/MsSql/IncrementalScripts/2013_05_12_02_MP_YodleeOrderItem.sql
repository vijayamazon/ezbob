IF OBJECT_ID ('dbo.MP_YodleeOrderItem') IS NOT NULL
	DROP TABLE dbo.MP_YodleeOrderItem
GO

CREATE TABLE dbo.MP_YodleeOrderItem
	(
     Id INT IDENTITY NOT NULL
    ,OrderId INT NOT NULL
    ,isSeidFromDataSource INT
    ,isSeidFromDataSourceSpecified BIT 
    ,isSeidMod INT
    ,isSeidModSpecified BIT 
    ,acctTypeId INT 
    ,acctTypeIdSpecified BIT 
    ,acctType NVARCHAR(300)
    ,localizedAcctType NVARCHAR(300)
    ,srcElementId NVARCHAR(300)
    ,individualInformationId INT 
    ,individualInformationIdSpecified BIT 
    ,bankAccountId INT 
    ,bankAccountIdSpecified BIT 
    ,customName NVARCHAR(300)
    ,customDescription NVARCHAR(300)
    ,isDeleted INT 
    ,isDeletedSpecified NVARCHAR(300)
    ,lastUpdated INT 
    ,lastUpdatedSpecified BIT 
    ,hasDetails INT 
    ,hasDetailsSpecified INT 
    ,interestRate FLOAT  
    ,interestRateSpecified FLOAT
    ,accountNumber NVARCHAR(300)
    ,link NVARCHAR(300)
    ,accountHolder NVARCHAR(300)
    ,tranListToDate DATETIME
    ,tranListFromDate DATETIME
    ,availableBalance FLOAT
    ,availableBalanceCurrency NVARCHAR(3)
    ,currentBalance FLOAT
    ,currentBalanceCurrency NVARCHAR(3)
    ,interestEarnedYtd FLOAT
    ,interestEarnedYtdCurrency NVARCHAR(3)
    ,prevYrInterest FLOAT 
    ,prevYrInterestCurrency NVARCHAR(3)
    ,overdraftProtection FLOAT
    ,overdraftProtectionCurrency NVARCHAR(3)
    ,term NVARCHAR(300)
    ,accountName NVARCHAR(300)
    ,annualPercentYield FLOAT 
    ,annualPercentYieldSpecified BIT 
    ,routingNumber NVARCHAR(300)
    ,maturityDate DATETIME
    ,asOfDate DATETIME
    --,bankTransactionsId INT -- class
    --,bankStatementsId INT -- class
    ,accountNicknameAtSrcSite NVARCHAR(300)
    ,isPaperlessStmtOn INT 
    ,isPaperlessStmtOnSpecified BIT 
    --,siteAccountStatusId INT --class
    ,siteAccountStatusSpecified BIT  
    ,created INT 
    ,createdSpecified BIT  
    ,nomineeName NVARCHAR(300)
    ,secondaryAccountHolderName NVARCHAR(300)
    ,accountOpenDate DATETIME
    ,accountCloseDate DATETIME
    ,maturityAmount FLOAT
    ,maturityAmountCurrency NVARCHAR(3)
    ,taxesWithheldYtd FLOAT
    ,taxesWithheldYtdCurrency NVARCHAR(3)
    ,taxesPaidYtd FLOAT
    ,taxesPaidYtdCurrency NVARCHAR(3)
    ,budgetBalance FLOAT
    ,budgetBalanceCurrency NVARCHAR(3)
    ,straightBalance FLOAT
    ,straightBalanceCurrency NVARCHAR(3)
    --,accountClassificationId INT --class 
    ,accountClassificationSpecified BIT 
    ,CONSTRAINT PK_MP_YodleeOrderItem PRIMARY KEY (Id)
   	,CONSTRAINT FK_MP_YodleeOrderItem_MP_YodleeOrder FOREIGN KEY (OrderId) REFERENCES dbo.MP_YodleeOrder (Id)
	)
GO

CREATE INDEX IX_MP_YodleeOrderItemOrderId
	ON dbo.MP_YodleeOrderItem (OrderId)
GO

