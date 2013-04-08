IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetExperianDefaultAccountId]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetExperianDefaultAccountId]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE GetExperianDefaultAccountId
AS
BEGIN
	SELECT ExperianAccountStatuses.Id FROM ExperianAccountStatuses WHERE DetailedStatus = 'Default'
END
GO
