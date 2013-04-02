SET QUOTED_IDENTIFIER ON
SET ARITHABORT ON
SET NUMERIC_ROUNDABORT OFF
SET CONCAT_NULL_YIELDS_NULL ON
SET ANSI_NULLS ON
SET ANSI_PADDING ON
SET ANSI_WARNINGS ON
GO
CREATE TABLE dbo.Tmp_LoanTransaction
	(
	Id int NOT NULL IDENTITY (1, 1),
	Type nvarchar(100) NOT NULL,
	PostDate datetime NOT NULL,
	Amount numeric(18, 2) NOT NULL,
	Description nvarchar(MAX) NOT NULL,
	LoanId int NOT NULL,
	Status nvarchar(50) NULL,
	TrackingNumber nvarchar(100) NULL,
	PacnetStatus nvarchar(1000) NULL,
	PaypointId nvarchar(1000) NULL,
	IP nvarchar(100) NULL,
	Principal numeric(18, 2) NULL,
	Interest numeric(18, 2) NULL,
	Fees numeric(18, 2) NULL,
	Balance numeric(18, 2) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_LoanTransaction SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_LoanTransaction ON
GO
IF EXISTS(SELECT * FROM dbo.LoanTransaction)
	 EXEC('INSERT INTO dbo.Tmp_LoanTransaction (Id, Type, PostDate, Amount, Description, LoanId, Status, TrackingNumber, PacnetStatus, PaypointId, IP, Principal, Interest, Fees, Balance)
		SELECT Id, Type, PostDate, Amount, Description, LoanId, Status, TrackingNumber, PacnetStatus, PaypointId, IP, Principal, Interest, Fees, Balance FROM dbo.LoanTransaction WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_LoanTransaction OFF
GO
DROP TABLE dbo.LoanTransaction
GO
EXECUTE sp_rename N'dbo.Tmp_LoanTransaction', N'LoanTransaction', 'OBJECT' 
GO
ALTER TABLE dbo.LoanTransaction ADD CONSTRAINT
	PK_LoanTransaction PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
CREATE TABLE dbo.Tmp_LoanTransaction
	(
	Id int NOT NULL IDENTITY (1, 1),
	Type nvarchar(100) NOT NULL,
	PostDate datetime NOT NULL,
	Amount numeric(18, 2) NOT NULL,
	Description nvarchar(MAX) NOT NULL,
	LoanId int NOT NULL,
	Status nvarchar(50) NULL,
	TrackingNumber nvarchar(100) NULL,
	PacnetStatus nvarchar(1000) NULL,
	PaypointId nvarchar(1000) NULL,
	IP nvarchar(100) NULL,
	Principal numeric(18, 2) NULL,
	Interest numeric(18, 2) NULL,
	Fees numeric(18, 2) NULL,
	Balance numeric(18, 2) NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.Tmp_LoanTransaction SET (LOCK_ESCALATION = TABLE)
GO
SET IDENTITY_INSERT dbo.Tmp_LoanTransaction ON
GO
IF EXISTS(SELECT * FROM dbo.LoanTransaction)
	 EXEC('INSERT INTO dbo.Tmp_LoanTransaction (Id, Type, PostDate, Amount, Description, LoanId, Status, TrackingNumber, PacnetStatus, PaypointId, IP, Principal, Interest, Fees, Balance)
		SELECT Id, Type, PostDate, Amount, Description, LoanId, Status, TrackingNumber, PacnetStatus, PaypointId, IP, Principal, Interest, Fees, Balance FROM dbo.LoanTransaction WITH (HOLDLOCK TABLOCKX)')
GO
SET IDENTITY_INSERT dbo.Tmp_LoanTransaction OFF
GO
DROP TABLE dbo.LoanTransaction
GO
EXECUTE sp_rename N'dbo.Tmp_LoanTransaction', N'LoanTransaction', 'OBJECT' 
GO
ALTER TABLE dbo.LoanTransaction ADD CONSTRAINT
	PK_LoanTransaction PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
