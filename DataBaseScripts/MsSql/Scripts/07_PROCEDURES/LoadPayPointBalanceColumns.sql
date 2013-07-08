IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[LoadPayPointBalanceColumns]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[LoadPayPointBalanceColumns]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE LoadPayPointBalanceColumns
AS
	SELECT
		name
	FROM
		syscolumns
	WHERE
		id = OBJECT_ID('PayPointBalance')
		AND
		name != 'Id'
	ORDER BY
		name
GO
