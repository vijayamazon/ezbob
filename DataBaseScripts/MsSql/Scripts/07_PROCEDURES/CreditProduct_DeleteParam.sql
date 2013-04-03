IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CreditProduct_DeleteParam]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CreditProduct_DeleteParam]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CreditProduct_DeleteParam]
  (
    @pId int
   )
AS
BEGIN
  if @pId is not null 
	begin
     delete creditproduct_params where [id] = @pId;
	end;
END;
GO
