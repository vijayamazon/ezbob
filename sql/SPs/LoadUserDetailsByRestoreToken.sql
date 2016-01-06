IF OBJECT_ID('LoadUserDetailsByRestoreToken') IS NULL
	EXECUTE('CREATE PROCEDURE LoadUserDetailsByRestoreToken AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadUserDetailsByRestoreToken
@Token UNIQUEIDENTIFIER,
@IsBrokerLead BIT
AS
BEGIN
	IF ISNULL(@IsBrokerLead, 0) = 0
	BEGIN
		SELECT 
			u.UserId,
			u.UserName,
			u.Email,
			u.OriginID
		FROM 
			Security_User u
			INNER JOIN CreatePasswordTokens t
				ON u.UserId = t.CustomerID
				AND t.TokenID = @Token
				AND t.DateAccessed IS NOT NULL
				AND t.DateDeleted IS NULL
	END
	ELSE BEGIN
		SELECT
			u.UserId,
			u.UserName,
			u.Email,
			u.OriginID
		FROM
			Security_User u
			INNER JOIN BrokerLeads l ON l.CustomerID = u.UserId
			INNER JOIN BrokerLeadTokens t
				ON l.BrokerLeadID = t.BrokerLeadID
				AND t.BrokerLeadToken = @Token
				AND t.DateAccessed IS NOT NULL
				AND t.DateDeleted IS NULL
	END
END
GO
