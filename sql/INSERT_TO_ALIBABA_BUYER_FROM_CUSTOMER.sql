/****** Script for SelectTopNRows command from SSMS  ******/
SELECT TOP 1000 [Id]
      ,[AliId]
      ,[CustomerId]
      ,[Freeze]
  FROM [prod_auto].[dbo].[AlibabaBuyer];


INSERT INTO [AlibabaBuyer] ([AliId], [CustomerId])
SELECT  Id , Id from dbo.Customer c where AlibabaId is not null and IsAlibaba =
 1 and CreditSum > 0 