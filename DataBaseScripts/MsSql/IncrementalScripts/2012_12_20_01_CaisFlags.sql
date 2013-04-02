CREATE TABLE [dbo].[CaisFlags](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[FlagSetting] [nvarchar](3) NULL,
	[Description] [nvarchar](50) NULL,
	[ValidForRecordType] [nvarchar](50) NULL,
	[Comment] [nvarchar](max) NULL,
 CONSTRAINT [PK_CaisFlags] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
