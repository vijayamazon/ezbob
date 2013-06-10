CREATE TABLE dbo.MP_TeraPeakCategory
	(
	Id int NOT NULL,
	Name nvarchar(1000) NOT NULL,
	FullName nvarchar(MAX) NOT NULL,
	[Level] int NOT NULL,
	ParentCategoryID int NOT NULL
	)  ON [PRIMARY]
	 TEXTIMAGE_ON [PRIMARY]
GO
ALTER TABLE dbo.MP_TeraPeakCategory ADD CONSTRAINT
	PK_MP_TeraPeakCategory PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.MP_TeraPeakCategory SET (LOCK_ESCALATION = TABLE)
GO

CREATE TABLE dbo.MP_TeraPeakCategoryStatistics
	(
	Id int NOT NULL,
	Listings int NOT NULL,
	Successful int NOT NULL,
	ItemsSold int NOT NULL,
	Revenue decimal(18, 4) NOT NULL,
	SuccessRate decimal(18, 4) NOT NULL,
	OrderItemId int NOT NULL,
	CategoryId int NOT NULL
	)  ON [PRIMARY]
GO

ALTER TABLE dbo.MP_TeraPeakCategoryStatistics ADD CONSTRAINT
	PK_MP_TeraPeakCategoryStatistics PRIMARY KEY CLUSTERED 
	(
	Id
	) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO

ALTER TABLE dbo.MP_TeraPeakCategoryStatistics SET (LOCK_ESCALATION = TABLE)
GO

ALTER TABLE dbo.MP_TeraPeakCategoryStatistics ADD CONSTRAINT
	FK_MP_TeraPeakCategoryStatistics_MP_TeraPeakCategory FOREIGN KEY
	(
	CategoryId
	) REFERENCES dbo.MP_TeraPeakCategory
	(
	Id
	) ON UPDATE  NO ACTION 
	 ON DELETE  NO ACTION 	
GO