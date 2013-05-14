IF OBJECT_ID ('dbo.DiscountPlan') IS NOT NULL
	DROP TABLE dbo.DiscountPlan
GO


CREATE TABLE dbo.DiscountPlan
    (
    Id int NOT NULL IDENTITY (1, 1),
    Name nvarchar(512) NULL,
    ValuesStr nvarchar(2048) NULL,
    IsDefault bit NULL
    )  ON [PRIMARY]
GO
ALTER TABLE dbo.DiscountPlan ADD CONSTRAINT
    PK_DiscountPlan PRIMARY KEY CLUSTERED 
    (
    Id
    ) WITH( STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]

GO
ALTER TABLE dbo.DiscountPlan SET (LOCK_ESCALATION = TABLE)
GO

INSERT INTO dbo.DiscountPlan (Name, ValuesStr, IsDefault) VALUES ('Simple Discount', '+0,+0,+0,-10,-20,-30', 'True')
GO