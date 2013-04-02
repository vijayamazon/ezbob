CREATE TABLE dbo.Bug
    (
    Id int NOT NULL,
    CustomerId int NULL,
    Type nvarchar(200) NULL,
    State nvarchar(200) NULL,
    MarketPlaceId int NULL,
    DateOpened datetime NULL,
    DateClosed datetime NULL,
    TextOpened nvarchar(2000) NULL,
    TextClosed nvarchar(2000) NULL,
    UnderwriterOpenedId int NULL,
    UnderwriterClosedId int NULL
    )  ON [PRIMARY]
GO
ALTER TABLE dbo.Bug ADD CONSTRAINT
    PK_Bug PRIMARY KEY CLUSTERED 
    (
    Id
    ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.Bug SET (LOCK_ESCALATION = TABLE)
GO
