CREATE TABLE dbo.AvailableFunds
    (
    Id int NOT NULL IDENTITY (1, 1),
    Date datetime NOT NULL,
    Value decimal(18, 4) NOT NULL
    )  ON [PRIMARY]
GO
ALTER TABLE dbo.AvailableFunds ADD CONSTRAINT
    PK_AvailableFunds PRIMARY KEY CLUSTERED 
    (
    Id
    ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.AvailableFunds SET (LOCK_ESCALATION = TABLE)
GO