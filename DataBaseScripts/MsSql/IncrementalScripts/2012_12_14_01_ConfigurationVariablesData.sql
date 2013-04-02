SET IDENTITY_INSERT [dbo].[ConfigurationVariables] ON 

GO
INSERT [dbo].[ConfigurationVariables] ([Id], [Name], [Value], [Description]) VALUES (1, N'CollectionPeriod1', N'7', N'first collection period')
GO
INSERT [dbo].[ConfigurationVariables] ([Id], [Name], [Value], [Description]) VALUES (3, N'CollectionPeriod2', N'14', N'second collection period')
GO
INSERT [dbo].[ConfigurationVariables] ([Id], [Name], [Value], [Description]) VALUES (4, N'CollectionPeriod3', N'30', N'third collection period')
GO
INSERT [dbo].[ConfigurationVariables] ([Id], [Name], [Value], [Description]) VALUES (5, N'LatePaymentCharge', N'20', N'A charge when an instalment is paid after 5 UK working days of the grace period')
GO
INSERT [dbo].[ConfigurationVariables] ([Id], [Name], [Value], [Description]) VALUES (6, N'RolloverCharge', N'50', N'A rollover has been agreed')
GO
INSERT [dbo].[ConfigurationVariables] ([Id], [Name], [Value], [Description]) VALUES (7, N'PartialPaymentCharge', N'45', N'A payment has been made (more than repayment interest + late payment fee but was not made in full)')
GO
INSERT [dbo].[ConfigurationVariables] ([Id], [Name], [Value], [Description]) VALUES (8, N'AdministrationCharge', N'75', N'A fee applied when no payment is received or less than (repayment interest + late payment fee)')
GO
INSERT [dbo].[ConfigurationVariables] ([Id], [Name], [Value], [Description]) VALUES (13, N'CollectionsCharge', N'10', N'10% of O/S balanse. Monthly charge applied when passed to collections under Sigma brand')
GO
SET IDENTITY_INSERT [dbo].[ConfigurationVariables] OFF
GO
