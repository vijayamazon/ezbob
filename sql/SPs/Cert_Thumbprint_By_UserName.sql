IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Cert_Thumbprint_By_UserName]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Cert_Thumbprint_By_UserName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Cert_Thumbprint_By_UserName] 
	(@pLogin        nvarchar(30))
AS
BEGIN
	select  certificateThumbprint as thumbprint, domainUserName
    from security_user
    WHERE UserName=@pLogin AND isdeleted != 2;
END
GO
