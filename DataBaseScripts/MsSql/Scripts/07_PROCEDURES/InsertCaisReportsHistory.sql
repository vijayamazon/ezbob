IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertCaisReportsHistory]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertCaisReportsHistory]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertCaisReportsHistory] 
(@Date datetime,
 @FileName nvarchar(25),
 @Type int,
 @NumberOfItems int, 
 @GoodUsers int,
 @DefaultUsers int,
 @FilePath nvarchar(400))
      
           
AS
BEGIN

insert INTO  [dbo].[CaisReportsHistory] (Date, FileName, Type, OfItems, GoodUsers, Defaults, UploadStatus, FilePath)
     VALUES (@Date, @FileName, @Type, @NumberOfItems, @GoodUsers, @DefaultUsers, 2, @FilePath);

SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO
