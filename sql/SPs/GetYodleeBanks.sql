IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetYodleeBanks]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetYodleeBanks]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetYodleeBanks] 
AS
BEGIN
SELECT ContentServiceId, Name FROM YodleeBanks WHERE Active=1
END
GO
