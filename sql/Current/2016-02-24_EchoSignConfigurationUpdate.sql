SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

SELECT Name, Value, Description, IsEncrypted INTO #t FROM ConfigurationVariables WHERE 1 = 0
GO

INSERT INTO 
	#t (Name, Value, Description, IsEncrypted) 
VALUES 
	('EchoSignClientId', '96A2AM24676Z7M', 'EchoSign OAuth Client Id', 0),  
	('EchoSignClientSecret', '98867629f92bfc28c8fd8df662d74626', 'EchoSign OAuth Client Secret', 0),
	('EchoSignRedirectUri', 'https://redirect.ezbob.com', 'Redirect uri specified in application configuration', 0),
	('EchoSignRefreshToken', '3AAABLblqZhABY-QxNfpgWJizd4ybsQa4hakBWBma-TYNXRyJYtAvqcU6TjK-zqtPRF4TqM-kLw0*', 'Used to get access token', 0);
GO
	
MERGE ConfigurationVariables AS TARGET
USING #t
 AS SOURCE
 ON (TARGET.Name = SOURCE.Name)
WHEN NOT MATCHED THEN INSERT (Name,Value,Description,IsEncrypted)
VALUES ( SOURCE.Name, SOURCE.Value, SOURCE.Description, SOURCE.IsEncrypted);
GO

DROP TABLE #t
GO
