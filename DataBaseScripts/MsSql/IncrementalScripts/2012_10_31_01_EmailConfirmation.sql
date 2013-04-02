ALTER TABLE dbo.Customer ADD
	EmailState nvarchar(100) NULL
GO

GO
CREATE TABLE dbo.EmailConfirmationRequest
	(
	Id uniqueidentifier NOT NULL,
	CustomerId int NOT NULL,
	Date datetime NOT NULL,
	State nvarchar(100) NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.EmailConfirmationRequest ADD CONSTRAINT
	PK_EmailConfirmationRequest PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.EmailConfirmationRequest SET (LOCK_ESCALATION = TABLE)
GO

UPDATE [dbo].[Customer]
   SET [EmailState] = 'Unknown'
 WHERE [EmailState] is null or [EmailState] = ''
GO