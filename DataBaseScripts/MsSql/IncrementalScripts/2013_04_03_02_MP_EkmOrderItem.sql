IF OBJECT_ID ('dbo.MP_EkmOrderItem') IS NOT NULL
	DROP TABLE dbo.MP_EkmOrderItem
GO

CREATE TABLE dbo.MP_EkmOrderItem
	(
	  Id                 INT IDENTITY NOT NULL
	, OrderId            INT
    , OrderNumber        NVARCHAR(300)
    , CustomerId         INT 
    , CompanyName        NVARCHAR(300)
    , FirstName          NVARCHAR(300)
    , LastName           NVARCHAR(300)
    , EmailAddress       NVARCHAR(300)
    , TotalCost          NUMERIC(18,2) 
    , OrderDate          NVARCHAR(300)
    , OrderStatus        NVARCHAR(300)
    , OrderStatusColour  NVARCHAR(300)
	, CONSTRAINT PK_MP_EkmOrderItem PRIMARY KEY (Id)
	, CONSTRAINT FK_MP_EkmOrderItem_MP_Order FOREIGN KEY (OrderId) REFERENCES dbo.MP_EkmOrder (Id)
	)
GO

CREATE INDEX IX_MP_EkmOrderItemOrderId
	ON dbo.MP_EkmOrderItem (OrderId)
GO

