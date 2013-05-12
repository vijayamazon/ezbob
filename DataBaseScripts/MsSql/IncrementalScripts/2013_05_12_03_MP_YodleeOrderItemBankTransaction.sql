IF OBJECT_ID ('dbo.MP_YodleeOrderItemBankTransaction') IS NOT NULL
	DROP TABLE dbo.MP_YodleeOrderItemBankTransaction
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

