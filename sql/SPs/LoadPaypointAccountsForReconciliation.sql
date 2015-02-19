SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadPaypointAccountsForReconciliation') IS NULL
	EXECUTE('CREATE PROCEDURE LoadPaypointAccountsForReconciliation AS SELECT 1')
GO

ALTER PROCEDURE LoadPaypointAccountsForReconciliation
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Mid,
		VpnPassword,
		RemotePassword
	FROM
		PayPointAccount
END
GO
