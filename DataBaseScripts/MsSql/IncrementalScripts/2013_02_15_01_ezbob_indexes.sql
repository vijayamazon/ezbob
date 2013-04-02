/****** Object:  Index [IX_CashRequests_IDCust]    Script Date: 02/11/2013 11:41:05 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[CashRequests]') 
AND name = N'IX_CashRequests_IDCust')
DROP INDEX [IX_CashRequests_IDCust] ON [dbo].[CashRequests] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_CashRequests_IDCust]    Script Date: 02/11/2013 11:41:05 ******/
CREATE NONCLUSTERED INDEX [IX_CashRequests_IDCust] ON [dbo].[CashRequests] 
(
	[Id] ASC,
	[IdCustomer] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, 
DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO


/****** Object:  Index [IX_Customer_Fill]    Script Date: 02/11/2013 11:41:27 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') 
AND name = N'IX_Customer_Fill')
DROP INDEX [IX_Customer_Fill] ON [dbo].[Customer] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_Customer_Fill]    Script Date: 02/11/2013 11:41:28 ******/
CREATE NONCLUSTERED INDEX [IX_Customer_Fill] ON [dbo].[Customer] 
(
	[IsSuccessfullyRegistered] ASC,
	[IsTest] ASC
)
INCLUDE ( [Id],
[CreditResult],
[FirstName],
[MiddleInitial],
[Surname],
[DateOfBirth],
[TimeAtAddress],
[ResidentialStatus],
[Gender],
[MartialStatus],
[TypeOfBusiness],
[DaytimePhone],
[MobilePhone],
[Fullname],
[OverallTurnOver],
[WebSiteTurnOver]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, 
IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
ON [PRIMARY]
GO


/****** Object:  Index [IX_Customer_IsRegistered]    Script Date: 02/11/2013 11:41:47 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Customer]') 
AND name = N'IX_Customer_IsRegistered')
DROP INDEX [IX_Customer_IsRegistered] ON [dbo].[Customer] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [IX_Customer_IsRegistered]    Script Date: 02/11/2013 11:41:47 ******/
CREATE NONCLUSTERED INDEX [IX_Customer_IsRegistered] ON [dbo].[Customer] 
(
	[IsSuccessfullyRegistered] ASC
)
INCLUDE ( [CreditResult]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, 
IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
ON [PRIMARY]
GO

/****** Object:  Index [IX_1]    Script Date: 02/11/2013 11:47:06 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[MP_CustomerMarketPlace]') 
AND name = N'IX_MP_CustomerMarketPlace_CUstId')
DROP INDEX [IX_MP_CustomerMarketPlace_CUstId] ON [dbo].[MP_CustomerMarketPlace] WITH ( ONLINE = OFF )
GO


/****** Object:  Index [IX_1]    Script Date: 02/11/2013 11:47:06 ******/
CREATE NONCLUSTERED INDEX [IX_MP_CustomerMarketPlace_CUstId] ON [dbo].[MP_CustomerMarketPlace] 
(
	[CustomerId] ASC,
	[UpdatingEnd] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, 
DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO


