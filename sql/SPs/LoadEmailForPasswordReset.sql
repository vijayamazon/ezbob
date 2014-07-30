IF OBJECT_ID('LoadEmailForPasswordReset') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEmailForPasswordReset AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadEmailForPasswordReset
@TargetID INT,
@Target NVARCHAR(32)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		Name AS Email
	FROM
		Customer
	WHERE
		Id = @TargetID
		AND
		@Target = 'Customer'
	UNION
	SELECT
		ContactEmail AS Email
	FROM
		Broker
	WHERE
		BrokerID = @TargetID
		AND
		@Target = 'Broker'
END
GO
