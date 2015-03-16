SET QUOTED_IDENTIFIER ON
GO

DROP INDEX MP_VatReturnSummary.IDX_VatReturnSummary
GO

CREATE NONCLUSTERED INDEX IDX_VatReturnSummary ON MP_VatReturnSummary (CustomerID, CustomerMarketplaceID) WHERE IsActive = 1
GO
