IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UnderwriterRecentCustomers]') AND type in (N'U'))
BEGIN
	CREATE TABLE [dbo].[UnderwriterRecentCustomers]
	(
		[Id] [int] IDENTITY(1,1) NOT NULL,
		[UserName] [nvarchar](100) NOT NULL,
		[CustomerId] [INT] NOT NULL,
	 CONSTRAINT [PK_UnderwriterRecentCustomers] PRIMARY KEY CLUSTERED 
	(
		[Id] ASC
	)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
	) ON [PRIMARY]
END
GO

