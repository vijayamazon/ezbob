IF OBJECT_ID ('dbo.MP_YodleeOrderItemBankTransaction') IS NOT NULL
	DROP TABLE dbo.MP_YodleeOrderItemBankTransaction
GO

IF OBJECT_ID ('dbo.MP_YodleeOrderItem') IS NOT NULL
	DROP TABLE dbo.MP_YodleeOrderItem
GO

IF OBJECT_ID ('dbo.MP_YodleeOrder') IS NOT NULL
	DROP TABLE dbo.MP_YodleeOrder
GO

CREATE TABLE dbo.MP_YodleeOrder
	(
	  Id                                         INT IDENTITY NOT NULL
	, CustomerMarketPlaceId                      INT NOT NULL
	, Created                                    DATETIME NOT NULL
	, CustomerMarketPlaceUpdatingHistoryRecordId INT
	, CONSTRAINT PK_MP_YodleeOrder PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_YodleeOrder_MP_CustomerMarketPlace FOREIGN KEY (CustomerMarketPlaceId) REFERENCES dbo.MP_CustomerMarketPlace (Id)
	)
GO

CREATE INDEX IX_MP_YodleeOrderCustomerMarketPlaceId
	ON dbo.MP_YodleeOrder (CustomerMarketPlaceId)
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
    ,isDeletedSpecified BIT
    ,lastUpdated INT 
    ,lastUpdatedSpecified BIT 
    ,hasDetails INT 
    ,hasDetailsSpecified BIT 
    ,interestRate FLOAT  
    ,interestRateSpecified BIT
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

CREATE TABLE dbo.MP_YodleeOrderItemBankTransaction
	(
   Id                  INT IDENTITY NOT NULL
  ,OrderItemId         INT NOT NULL
  ,isSeidFromDataSource INT
  ,isSeidFromDataSourceSpecified BIT 
  ,isSeidMod INT
  ,isSeidModSpecified BIT 
  ,srcElementId NVARCHAR(300) 
  ,transactionTypeId INT
  ,transactionTypeIdSpecified BIT 
  ,transactionType NVARCHAR(300) 
  ,localizedTransactionType NVARCHAR(300) 
  ,transactionStatusId INT
  ,transactionStatusIdSpecified BIT 
  ,transactionStatus NVARCHAR(300) 
  ,localizedTransactionStatus NVARCHAR(300) 
  ,transactionBaseTypeId INT
  ,transactionBaseTypeIdSpecified BIT 
  ,transactionBaseType NVARCHAR(300) 
  ,localizedTransactionBaseType NVARCHAR(300) 
  ,categoryId INT
  ,categoryIdSpecified BIT 
  ,bankTransactionId INT
  ,bankTransactionIdSpecified BIT 
  ,bankAccountId INT
  ,bankAccountIdSpecified BIT 
  ,bankStatementId INT
  ,bankStatementIdSpecified BIT 
  ,isDeleted INT
  ,isDeletedSpecified BIT 
  ,lastUpdated INT
  ,lastUpdatedSpecified BIT 
  ,hasDetails INT
  ,hasDetailsSpecified BIT 
  ,transactionId NVARCHAR(300) 
  ,transactionCategoryId NVARCHAR(300) 
  ,siteCategoryType NVARCHAR(300) 
  ,siteCategory NVARCHAR(300) 
  ,classUpdationSource NVARCHAR(300) 
  ,lastCategorised NVARCHAR(300) 
  ,transactionDate DATETIME 
  ,isReimbursable INT
  ,isReimbursableSpecified BIT 
  ,mcCode NVARCHAR(300) 
  ,prevLastCategorised INT
  ,prevLastCategorisedSpecified BIT 
  ,naicsCode NVARCHAR(300) 
  ,runningBalance FLOAT 
  ,runningBalanceCurrency NVARCHAR(3)
  ,userDescription NVARCHAR(300) 
  ,customCategoryId INT
  ,customCategoryIdSpecified BIT 
  ,memo NVARCHAR(300) 
  ,parentId INT
  ,parentIdSpecified BIT 
  ,isOlbUserDesc INT
  ,isOlbUserDescSpecified BIT 
  ,categorisationSourceId NVARCHAR(300) 
  ,plainTextDescription NVARCHAR(300) 
  ,splitType NVARCHAR(300) 
  ,categoryLevelId INT
  ,categoryLevelIdSpecified BIT 
  ,calcRunningBalance FLOAT 
  ,calcRunningBalanceCurrency NVARCHAR(3)
  ,category NVARCHAR(300) 
  ,link NVARCHAR(300) 
  ,postDate DATETIME 
  ,prevTransactionCategoryId INT
  ,prevTransactionCategoryIdSpecified BIT 
  ,isBusinessExpense INT
  ,isBusinessExpenseSpecified BIT 
  ,descriptionViewPref INT
  ,descriptionViewPrefSpecified BIT 
  ,prevCategorisationSourceId INT
  ,prevCategorisationSourceIdSpecified BIT 
  ,transactionAmount FLOAT 
  ,transactionAmountCurrency NVARCHAR(3)
  ,transactionPostingOrder INT
  ,transactionPostingOrderSpecified BIT 
  ,checkNumber NVARCHAR(300) 
  ,description NVARCHAR(300) 
  ,isTaxDeductible INT
  ,isTaxDeductibleSpecified BIT 
  ,isMedicalExpense INT
  ,isMedicalExpenseSpecified BIT 
  ,categorizationKeyword NVARCHAR(300) 
  ,sourceTransactionType NVARCHAR(300) 
  , CONSTRAINT PK_MP_YodleeOrderItemBankTransaction PRIMARY KEY (Id)
  , CONSTRAINT FK_MP_YodleeOrderItemBankTransaction_MP_YodleeOrderItem FOREIGN KEY (OrderItemId) REFERENCES dbo.MP_YodleeOrderItem (Id)
 )
GO

CREATE INDEX IX_MP_YodleeOrderItemBankTransactionOrderItemId
	ON dbo.MP_YodleeOrderItemBankTransaction (OrderItemId)
GO

