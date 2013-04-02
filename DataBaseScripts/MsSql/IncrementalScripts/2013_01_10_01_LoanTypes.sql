CREATE TABLE dbo.LoanType
    (
    Id int NOT NULL,
    Type nvarchar(50) NOT NULL,
    Name nvarchar(250) NOT NULL,
    Description nvarchar(MAX) NULL,
    IsDefault bit NOT NULL
    )  ON [PRIMARY]
     TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.LoanType ADD CONSTRAINT
    PK_LoanType PRIMARY KEY CLUSTERED 
    (
    Id
    ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.LoanType SET (LOCK_ESCALATION = TABLE)
GO

INSERT INTO [dbo].[LoanType]
           ([Id]
           ,[Type]
           ,[Name]
           ,[Description]
           ,[IsDefault])
     VALUES
           (1
           ,'StandardLoanType'
           ,'Standard Loan'
           ,'Standard Loan'
           ,1)
GO


INSERT INTO [dbo].[LoanType]
           ([Id]
           ,[Type]
           ,[Name]
           ,[Description]
           ,[IsDefault])
     VALUES
           (2
           ,'HalfWayLoanType'
           ,'HalfWay Loan'
           ,'HalfWay Loan'
           ,0)
GO

ALTER TABLE dbo.Loan ADD
    LoanTypeId int NULL
GO

ALTER TABLE dbo.CashRequests ADD
    LoanTypeId int NULL
GO

ALTER TABLE dbo.DecisionHistory ADD
  LoanTypeId int NULL
GO

UPDATE [dbo].[Loan]
   SET [LoanTypeId] = 1
 WHERE LoanTypeId is null

GO

UPDATE [dbo].[CashRequests]
   SET [LoanTypeId] = 1
 WHERE LoanTypeId is null

 GO
 
UPDATE [dbo].[DecisionHistory]
   SET [LoanTypeId] = 1
 WHERE LoanTypeId is null