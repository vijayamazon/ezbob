DROP TABLE [dbo].[LoanCharges]
GO

CREATE TABLE [dbo].[LoanCharges](
    [Id] [int] NOT NULL,
    [Amount] [decimal](18, 4) NOT NULL,
    [LoanId] [int] NOT NULL,
    [ConfigurationVariableId] [int] NULL,
    [Date] [datetime] NULL,
 CONSTRAINT [PK_LoanCharges] PRIMARY KEY CLUSTERED 
(
    [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

GO