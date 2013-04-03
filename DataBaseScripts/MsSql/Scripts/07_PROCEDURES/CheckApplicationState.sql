IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[CheckApplicationState]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[CheckApplicationState]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Protsyuk Oleg
-- Create date: 01.02.2008
-- Description:	Џа®ўҐаЄ  б®бв®п­Ёп § пўЄЁ, Ґб«Ё § пўЄ  ®Ўа Ў влў Ґвбп SE Ўа®б Ґ¬ ЁбЄ«озҐ­ЁҐ
-- =============================================
CREATE PROCEDURE CheckApplicationState
	@pApplicationId bigint
AS
BEGIN
  DECLARE @lState bigint
  Select @lState = State from Application_Application where ApplicationId  = @pApplicationId;

 if @lState is NULL
	 RAISERROR ('ApplicationIdDoesNotExist', 11, 1);
	 
 if @lState = 0
     RAISERROR('ApplicationExecutingByEngine', 11, 1);
END
GO
