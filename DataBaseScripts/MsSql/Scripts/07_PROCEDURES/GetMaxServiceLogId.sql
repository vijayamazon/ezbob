IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetMaxServiceLogId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetMaxServiceLogId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE GetMaxServiceLogId
	@UpperBound BIGINT
AS
BEGIN
	DECLARE @MaxId BIGINT
	SELECT @MaxId = max(Id) FROM MP_ServiceLog
	SELECT max(Id) AS MaxIdInBounds, @MaxId AS MaxId FROM MP_ServiceLog WHERE Id <= @UpperBound
END
GO
