IF OBJECT_ID('ChangeBrokerEmail') IS NULL
	EXECUTE('CREATE PROCEDURE ChangeBrokerEmail AS SELECT 1')
GO

ALTER PROCEDURE ChangeBrokerEmail
	(@OldEmail NVARCHAR(255),
	 @NewEmail NVARCHAR(255),
	 @NewPassword NVARCHAR(255))
AS
BEGIN
	
DECLARE @UserId INT
SELECT @UserId = BrokerId FROM Broker WHERE ContactEmail = @OldEmail

UPDATE EzbobMailNodeAttachRelation SET ToField = @NewEmail WHERE ToField = @OldEmail
UPDATE Broker SET ContactEmail = @NewEmail, Password = @NewPassword WHERE ContactEmail = @OldEmail
UPDATE Security_User SET UserName = @NewEmail, EMail = @NewEmail, EzPassword = @NewPassword WHERE UserId = @UserId

END
GO
