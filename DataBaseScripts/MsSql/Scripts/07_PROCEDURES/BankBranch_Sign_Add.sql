IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BankBranch_Sign_Add]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[BankBranch_Sign_Add]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [BankBranch_Sign_Add]
(
    @pUserId int
    ,@pdata ntext
    ,@psignedDocument nvarchar(max)
)
AS

BEGIN
    INSERT INTO [BankBranch_Sign]
      ([UserId]
      ,[ModificationTime]
      ,[Data]
      ,[SignedDocument])
    VALUES
      (@puserId
      ,GETDATE()
      ,@pdata
      ,@psignedDocument);
END
GO
