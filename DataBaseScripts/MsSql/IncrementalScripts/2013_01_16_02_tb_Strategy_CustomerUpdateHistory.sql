/****** Object:  Table [dbo].[Strategy_CustomerUpdateHistory]    Script Date: 12/04/2012 14:19:53 ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

/*
-- =============================================
-- Author:		Oleg Zemskyi
-- Create date: 04.12.2012
-- Description:	Update customers with strategy
-- =============================================
*/

CREATE TABLE [dbo].[Strategy_CustomerUpdateHistory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerId] [int] NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL
) ON [PRIMARY]

GO


