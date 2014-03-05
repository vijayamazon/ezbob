IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetSignals]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetSignals]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetSignals] 
	(@pCount int)
AS
BEGIN
	update Signal 
  set Status = -1
  where id in
    (select TOP(@pCount) id 
     from Signal 
     where status= 0 and (IsExternal is null or IsExternal = 0) 
     order by priority);

  SELECT 
    applicationId,
    appSpecific,
    id,
    label,
    priority,
    ownerApplicationId,
    executionType,
    message body
  FROM
    signal WITH (NOLOCK)
  WHERE
    status = -1
  order by priority
END
GO
