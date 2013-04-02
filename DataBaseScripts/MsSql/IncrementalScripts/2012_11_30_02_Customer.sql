go
ALTER TABLE dbo.Customer ADD
	[CollectionDateOfDeclaration] [datetime] NULL,
	[IsAddCollectionFee] [bit] NULL,
	[CollectionFee] [decimal](18, 0) NULL,
	[CollectionDescription] [nvarchar](50) NULL
