IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ResetAllSignals]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[ResetAllSignals]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ResetAllSignals]
as
begin
    update Signal
    set Status = 0 
    where IsExternal is null or IsExternal = 0 
END;
GO
