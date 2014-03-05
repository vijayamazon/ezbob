IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Log_TraceLogInsert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Log_TraceLogInsert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Log_TraceLogInsert] 
	(@pApplicationId int, 
   @pType int, 
   @pCode int, 
   @pMessage ntext, 
   @pData ntext, 
   @pInsertDate datetime, 
   @pThreadId int)
AS
BEGIN
	INSERT INTO Log_TraceLog (
                              ApplicationId, Type, Code, 
                              Message, Data, InsertDate, ThreadId
                              ) 
    VALUES (@pApplicationId, @pType, @pCode, 
            @pMessage, @pData, @pInsertDate, @pThreadId)
END
GO
