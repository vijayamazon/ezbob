ALTER TABLE Customer
ADD CONSTRAINT FK_Customer_CustomerStatuses FOREIGN KEY (CollectionStatus) REFERENCES dbo.CustomerStatuses (Id)

