IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrokerLoadContactData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[BrokerLoadContactData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BrokerLoadContactData] 
	(@BrokerID INT)
AS
BEGIN
	SELECT
		BrokerID,
		ContactName,
		ContactEmail
	FROM
		Broker
	WHERE
		BrokerID = @BrokerID
END
GO
