IF OBJECT_ID('Greeting_Mail_Sent') IS NULL
	EXECUTE('CREATE PROCEDURE Greeting_Mail_Sent AS SELECT 1')
GO

ALTER PROCEDURE Greeting_Mail_Sent
@UserId INT,
@GreetingMailSent INT,
@Now DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	UPDATE Customer SET
		GreetingMailSent = @GreetingMailSent,
		GreetingMailSentDate = ISNULL(GreetingMailSentDate, @Now)
	WHERE
		Id = @UserId
END
GO
