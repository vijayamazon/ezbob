IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CompanyEmployeeCount]') AND type in (N'U'))
DROP TABLE [dbo].[CompanyEmployeeCount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CompanyEmployeeCount](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NOT NULL,
	[Created] [datetime] NOT NULL,
	[EmployeeCount] [int] NOT NULL,
	[TopEarningEmployeeCount] [int] NOT NULL,
	[BottomEarningEmployeeCount] [int] NOT NULL,
	[EmployeeCountChange] [int] NOT NULL,
 CONSTRAINT [PK_CompanyEmployeeCount] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CompanyEmployeeCount] ADD  CONSTRAINT [DF_CompanyEmployeeCount_Created]  DEFAULT (getdate()) FOR [Created]
GO
