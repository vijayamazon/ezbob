﻿IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertExperianAccount]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertExperianAccount]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE InsertExperianAccount
	@ServiceLogId BIGINT,
	@CustomerId BIGINT,
	@AccountType VARCHAR(100),
	@DefMonth DATETIME,
	@Balance INT,
	@CurrentDefBalance INT
AS
BEGIN
	INSERT INTO ExperianDefaultAccountsData (ServiceLogId, CustomerId, AccountType, DefMonth, Balance, CurrentDefBalance) 
	VALUES (@ServiceLogId, @CustomerId, @AccountType, @DefMonth, @Balance, @CurrentDefBalance)
END
GO
