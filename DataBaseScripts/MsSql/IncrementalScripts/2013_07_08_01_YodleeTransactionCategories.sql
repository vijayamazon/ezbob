CREATE TABLE dbo.MP_YodleeTransactionCategories
	(
	  Id                              INT IDENTITY NOT NULL
	, CategoryId                      NVARCHAR(300) NOT NULL 
	, Name                            NVARCHAR(300) NOT NULL
	, Type                            NVARCHAR(300) NOT NULL
	, CONSTRAINT PK_MP_YodleeTransactionCategories PRIMARY KEY (CategoryId)
	)
GO

CREATE INDEX IX_MP_YodleeTransactionCategory
	ON dbo.MP_YodleeTransactionCategories (CategoryId)
GO

INSERT INTO MP_YodleeTransactionCategories VALUES (1,'Uncategorized','Uncategorized')
INSERT INTO MP_YodleeTransactionCategories VALUES (100,'Advertising','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (25,'ATM/Cash Withdrawals','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (2,'Automotive Expenses','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (102,'Business Miscellaneous','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (15,'Cable/Satellite Services','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (3,'Charitable Giving','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (33,'Checks','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (4,'Child/Dependent Expenses','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (5,'Clothing/Shoes','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (108,'Dues and Subscriptions','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (6,'Education','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (43,'Electronics','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (7,'Entertainment','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (8,'Gasoline/Fuel','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (44,'General Merchandise','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (9,'Gifts','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (10,'Groceries','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (11,'Healthcare/Medical','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (34,'Hobbies','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (13,'Home Improvement','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (12,'Home Maintenance','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (14,'Insurance','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (17,'Loans','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (18,'Mortgages','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (110,'Office Maintenance','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (45,'Office Supplies','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (16,'Online Services','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (35,'Other Bills','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (19,'Other Expenses','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (20,'Personal Care','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (42,'Pets/Pet Care','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (104,'Postage and Shipping','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (106,'Printing','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (21,'Rent','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (22,'Restaurants/Dining','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (24,'Service Charges/Fees','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (37,'Taxes','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (38,'Telephone Services','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (23,'Travel','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (39,'Utilities','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (112,'Wages Paid','Expenses')
INSERT INTO MP_YodleeTransactionCategories VALUES (92,'Consulting','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (27,'Deposits','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (114,'Expense Reimbursement','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (96,'Interest','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (30,'Investment Income','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (32,'Other Income','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (29,'Paychecks/Salary','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (31,'Retirement Income','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (94,'Sales','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (98,'Services','Income')
INSERT INTO MP_YodleeTransactionCategories VALUES (26,'Credit Card Payments','Transfers')
INSERT INTO MP_YodleeTransactionCategories VALUES (40,'Savings','Transfers')
INSERT INTO MP_YodleeTransactionCategories VALUES (36,'Securities Trades','Transfers')
INSERT INTO MP_YodleeTransactionCategories VALUES (28,'Transfers','Transfers')
INSERT INTO MP_YodleeTransactionCategories VALUES (41,'Retirement Contributions','Deferred Compensation')
GO

ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction]  WITH CHECK ADD CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeTransactionCategories] FOREIGN KEY([transactionCategoryId])
REFERENCES [dbo].[MP_YodleeTransactionCategories] ([CategoryId])
GO

ALTER TABLE [dbo].[MP_YodleeOrderItemBankTransaction] CHECK CONSTRAINT [FK_MP_YodleeOrderItemBankTransaction_MP_YodleeTransactionCategories]
GO

    
    
