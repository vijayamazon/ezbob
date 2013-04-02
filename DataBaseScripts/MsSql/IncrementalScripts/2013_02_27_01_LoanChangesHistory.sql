CREATE TABLE dbo.LoanChangesHistory
    (
    Id int NOT NULL,
    Date datetime NOT NULL,
    LoanId int NOT NULL,
    Data nvarchar(MAX) NOT NULL,
    UserId int NOT NULL
    )  ON [PRIMARY]
     TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.LoanChangesHistory ADD CONSTRAINT
    PK_LoanChangesHistory PRIMARY KEY CLUSTERED 
    (
    Id
    ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.LoanChangesHistory SET (LOCK_ESCALATION = TABLE)
GO