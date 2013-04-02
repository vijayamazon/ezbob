SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
GO
CREATE TABLE dbo.Tmp_LoanSchedule
	(
	Id int NOT NULL IDENTITY (1, 1),
	Date datetime NOT NULL,
	RepaymentAmount numeric(18, 2) NOT NULL,
	Interest numeric(18, 2) NOT NULL,
	Status nvarchar(50) NOT NULL,
	LateCharges numeric(18, 2) NOT NULL,
	AmountDue numeric(18, 2) NOT NULL,
	LoanId int NOT NULL,
	Position int NULL,
	Principal numeric(18, 2) NULL,
	Balance decimal(18, 2) NULL,
	LoanRepayment decimal(18, 2) NULL,
	Delinquency int NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_LoanSchedule SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_LoanSchedule ON
GO
IF EXISTS(SELECT * FROM dbo.LoanSchedule)
	 EXEC('INSERT INTO dbo.Tmp_LoanSchedule (Id, Date, RepaymentAmount, Interest, Status, LateCharges, AmountDue, LoanId, Position, Principal, Balance, LoanRepayment, Delinquency)
		SELECT Id, Date, RepaymentAmount, Interest, Status, LateCharges, AmountDue, LoanId, Position, Principal, Balance, LoanRepayment, Delinquency FROM dbo.LoanSchedule WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_LoanSchedule OFF
GO
DROP TABLE dbo.LoanSchedule
GO
EXECUTE sp_rename N'dbo.Tmp_LoanSchedule', N'LoanSchedule', 'OBJECT' 
GO
ALTER TABLE dbo.LoanSchedule ADD CONSTRAINT
	PK_LoanSchedule PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO