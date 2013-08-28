IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[gsm_numbers_1101_1307]') AND type in (N'U'))
DROP TABLE [dbo].[gsm_numbers_1101_1307]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[gsm_numbers_1101_1307](
	[c_number] [decimal](18, 0) NULL,
	[c_initial] [datetime2](3) NULL
) ON [PRIMARY]
GO
