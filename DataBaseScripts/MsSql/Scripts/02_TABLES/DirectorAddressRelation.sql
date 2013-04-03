IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[DirectorAddressRelation]') AND type in (N'U'))
DROP TABLE [dbo].[DirectorAddressRelation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[DirectorAddressRelation](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[DirectorId] [int] NOT NULL,
	[addressId] [int] NOT NULL
) ON [PRIMARY]
GO
