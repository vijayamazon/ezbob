/****** Object:  Index [i1]    Script Date: 10/05/2012 16:18:18 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Application_Detail]') AND name = N'i1')
DROP INDEX [i1] ON [dbo].[Application_Detail] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [i1]    Script Date: 10/05/2012 16:18:19 ******/
CREATE NONCLUSTERED INDEX [i1] ON [dbo].[Application_Detail] 
(
	[ApplicationId] ASC
)
INCLUDE ( [DetailNameId]) 
GO


/****** Object:  Index [i3]    Script Date: 10/05/2012 16:19:40 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Application_Detail]') AND name = N'i3')
DROP INDEX [i3] ON [dbo].[Application_Detail] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [i3]    Script Date: 10/05/2012 16:19:40 ******/
CREATE NONCLUSTERED INDEX [i3] ON [dbo].[Application_Detail] 
(
	[ParentDetailId] ASC
)
INCLUDE ( [DetailNameId])
GO


/****** Object:  Index [i_app_hist_1]    Script Date: 10/05/2012 16:38:33 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Application_History]') AND name = N'i_app_hist_1')
DROP INDEX [i_app_hist_1] ON [dbo].[Application_History] WITH ( ONLINE = OFF )
GO

/****** Object:  Index [i_app_hist_1]    Script Date: 10/05/2012 16:38:33 ******/
CREATE NONCLUSTERED INDEX [i_app_hist_1] ON [dbo].[Application_History] 
(
	[UserId] ASC
)
INCLUDE ( [AppHistoryId],
[ActionDateTime],
[CurrentNodeID],
[ActionType],
[ApplicationId]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

/****** Object:  Index [IX_Application_Application_LBU]    Script Date: 10/05/2012 16:39:12 ******/
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Application_Application]') AND name = N'IX_Application_Application_LBU')
DROP INDEX [IX_Application_Application_LBU] ON [dbo].[Application_Application] WITH ( ONLINE = OFF )
GO


/****** Object:  Index [IX_Application_Application_LBU]    Script Date: 10/05/2012 16:39:12 ******/
CREATE NONCLUSTERED INDEX [IX_Application_Application_LBU] ON [dbo].[Application_Application] 
(
	[LockedByUserId] ASC
)
INCLUDE ( [ApplicationId],
[CreationDate],
[CreatorUserId],
[StrategyId],
[Version],
[AppCounter]) WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO

