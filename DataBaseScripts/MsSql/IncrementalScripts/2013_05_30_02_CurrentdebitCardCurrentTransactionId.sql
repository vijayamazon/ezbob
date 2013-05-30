update Customer set CurrentDebitCard = (SELECT top 1 ci.[Id] from [CardInfo] ci where ci.[CustomerId] = Customer.Id) where CurrentDebitCard is null
update Customer set PayPointTransactionId = (SELECT top 1 p.TransactionId from PayPointCard p where p.[CustomerId] = Customer.Id) where PayPointTransactionId is null