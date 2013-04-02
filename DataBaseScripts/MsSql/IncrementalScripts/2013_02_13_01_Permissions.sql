delete from [dbo].[Security_RolePermissionRel] where PermissionId = 9 and RoleId = 33
GO
INSERT [dbo].[Security_RolePermissionRel] ([RoleId], [PermissionId]) VALUES (33, 10)
GO
