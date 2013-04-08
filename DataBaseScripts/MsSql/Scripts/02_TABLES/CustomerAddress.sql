IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CustomerAddress]') AND type in (N'U'))
DROP TABLE [dbo].[CustomerAddress]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerAddress](
	[addressId] [int] IDENTITY(1,1) NOT NULL,
	[addressType] [int] NULL,
	[id] [varchar](50) NULL,
	[Organisation] [varchar](200) NULL,
	[Line1] [varchar](200) NULL,
	[Line2] [varchar](200) NULL,
	[Line3] [varchar](200) NULL,
	[Town] [varchar](200) NULL,
	[County] [varchar](200) NULL,
	[Postcode] [varchar](200) NULL,
	[Country] [varchar](200) NULL,
	[Rawpostcode] [varchar](200) NULL,
	[Deliverypointsuffix] [varchar](200) NULL,
	[Nohouseholds] [varchar](200) NULL,
	[Smallorg] [varchar](200) NULL,
	[Pobox] [varchar](200) NULL,
	[Mailsortcode] [varchar](200) NULL,
	[Udprn] [varchar](200) NULL
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_CustomerAddress_ID] ON [dbo].[CustomerAddress] 
(
	[addressId] ASC,
	[addressType] ASC
)
INCLUDE ( [id],
[Organisation],
[Line1],
[Line2],
[Line3],
[Town],
[County],
[Postcode],
[Country],
[Rawpostcode],
[Deliverypointsuffix],
[Nohouseholds],
[Smallorg],
[Pobox],
[Mailsortcode],
[Udprn]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
