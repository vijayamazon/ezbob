IF OBJECT_ID('AlibabaBuyer') IS NULL
BEGIN

	CREATE TABLE [dbo].[AlibabaBuyer](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[AliId] [nvarchar](50) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[Freeze] [decimal](18, 2) NULL,
	 CONSTRAINT [PK_AlibabaBuyer] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
	) ON [PRIMARY]

END
GO

BEGIN	
alter table AlibabaBuyer alter column AliId numeric(20) not null;
END
GO

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_AlibabaBuyerCustomer')
BEGIN	
	ALTER TABLE [dbo].[AlibabaBuyer]  WITH NOCHECK ADD  CONSTRAINT [FK_AlibabaBuyerCustomer] FOREIGN KEY([CustomerId]) REFERENCES [dbo].[Customer] ([Id])
END
GO