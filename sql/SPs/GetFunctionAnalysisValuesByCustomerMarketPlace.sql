IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetFunctionAnalysisValuesByCustomerMarketPlace]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetFunctionAnalysisValuesByCustomerMarketPlace]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetFunctionAnalysisValuesByCustomerMarketPlace] 
	(@MpID INT)
AS
BEGIN
		SET NOCOUNT ON

	SELECT
		f.internalid AS fid,
		p.internalid AS fpid,
		v.value,
		h.updatingstart
	FROM
		mp_analyisisfunctionvalues                       v
		INNER JOIN (
			SELECT id, updatingstart
			FROM mp_customermarketplaceupdatinghistory
			WHERE updatingstart IS NOT NULL
			AND updatingend IS NOT NULL
		)
		                                                 h ON v.customermarketplaceupdatinghistoryrecordid = h.id
		INNER JOIN mp_customermarketplace                m ON v.customermarketplaceid = m.id
		INNER JOIN mp_analyisisfunction                  f ON v.analyisisfunctionid = f.id
		INNER JOIN mp_analysisfunctiontimeperiod         p ON v.analysisfunctiontimeperiodid = p.id
	WHERE
		m.id = @MpID
END
GO
