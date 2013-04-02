CREATE NONCLUSTERED INDEX [IX_CustomerAddress_ID] ON [dbo].[CustomerAddress]
(
	[addressId] ASC,
	[addressType] ASC
)
INCLUDE ( 	[id],
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
	[Udprn]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_CustomerAddressRelation_CustId] ON [dbo].[CustomerAddressRelation] 
(
 [customerId] ASC
)
INCLUDE ( [id],
[addressId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO