--удаляем индекс (потому как на него ругалось)
IF  EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID(N'[dbo].[Security_User]') AND name = N'IX_SECURITY_USER')
ALTER TABLE [dbo].[Security_User] DROP CONSTRAINT [IX_SECURITY_USER]
GO
-- меняем поле
ALTER TABLE Security_User ALTER COLUMN UserName nvarchar(250) NOT NULL
-- создаем индекс заново
ALTER TABLE [dbo].[Security_User] ADD  CONSTRAINT [IX_SECURITY_USER] UNIQUE NONCLUSTERED 
(
 [UserName] ASC,
 [DeleteId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
GO