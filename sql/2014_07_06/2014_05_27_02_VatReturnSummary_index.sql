SET QUOTED_IDENTIFIER ON
GO

DROP INDEX MP_VatReturnSummary.IDX_VatReturnSummary
GO

CREATE INDEX IDX_VatReturnSummary ON MP_VatReturnSummary(CustomerID, CustomerMarketplaceID, BusinessID) WHERE IsActive = 1
GO
