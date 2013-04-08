IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_User]') AND type in (N'U'))
DROP TABLE [dbo].[Security_User]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Security_User](
	[UserId] [int] IDENTITY(1,1) NOT NULL,
	[UserName] [nvarchar](250) NOT NULL,
	[FullName] [nvarchar](250) NOT NULL,
	[Password] [nvarchar](200) NULL,
	[CreationDate] [datetime] NOT NULL,
	[IsDeleted] [int] NOT NULL,
	[EMail] [nvarchar](255) NULL,
	[CreateUserId] [int] NULL,
	[DeletionDate] [datetime] NULL,
	[DeleteUserId] [int] NULL,
	[BranchId] [int] NOT NULL,
	[PassSetTime] [datetime] NULL,
	[LoginFailedCount] [int] NULL,
	[DisableDate] [datetime] NULL,
	[LastBadLogin] [datetime] NULL,
	[PassExpPeriod] [bigint] NULL,
	[ForcePassChange] [int] NULL,
	[DisablePassChange] [int] NULL,
	[DeleteId] [int] NULL,
	[CertificateThumbprint] [nvarchar](40) NULL,
	[DomainUserName] [nvarchar](250) NULL,
	[SecurityQuestion1Id] [bigint] NULL,
	[SecurityAnswer1] [varchar](200) NULL,
	[IsPasswordRestored] [bit] NULL,
 CONSTRAINT [PK_Security_User] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY],
 CONSTRAINT [IX_SECURITY_USER] UNIQUE NONCLUSTERED 
(
	[UserName] ASC,
	[DeleteId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Security_User] ADD  CONSTRAINT [DF_AppUser_CreationDate]  DEFAULT (getdate()) FOR [CreationDate]
GO
ALTER TABLE [dbo].[Security_User] ADD  CONSTRAINT [DF_AppUser_IsDeleted]  DEFAULT ((0)) FOR [IsDeleted]
GO
ALTER TABLE [dbo].[Security_User] ADD  CONSTRAINT [DF_Security_User_PassSetTime]  DEFAULT (getdate()) FOR [PassSetTime]
GO
