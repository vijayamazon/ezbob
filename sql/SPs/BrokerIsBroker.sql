IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[BrokerIsBroker]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[BrokerIsBroker]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[BrokerIsBroker] 
	(@ContactEmail NVARCHAR(255))
AS
BEGIN
	SET NOCOUNT ON;

	DECLARE @BrokerID INT

	SELECT
		@BrokerID = BrokerID
	FROM
		Broker
	WHERE
		ContactEmail = @ContactEmail

	SELECT ISNULL(@BrokerID, 0) AS BrokerID
END
GO
