CREATE TABLE dbo.DecisionHistory
	(
	Id int NOT NULL,
	Action nvarchar(50) NOT NULL,
	Date datetime NOT NULL,
	Comment nvarchar(2000) NULL,
	UnderwriterId int NOT NULL,
	CustomerId int NOT NULL
	)  ON [PRIMARY]
GO
ALTER TABLE dbo.DecisionHistory ADD CONSTRAINT
	PK_DecisionHistory PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO