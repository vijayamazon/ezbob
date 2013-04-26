ALTER TABLE CustomerAddress
	ADD CustomerId INT

ALTER TABLE CustomerAddress
	ADD DirectorId INT

go

MERGE INTO CustomerAddress
   USING (
          SELECT addressId, DirectorId 
            FROM DirectorAddressRelation           
         ) AS source
      ON CustomerAddress.addressId = source.addressId
WHEN MATCHED THEN
   UPDATE 
      SET CustomerAddress.DirectorId=source.DirectorId;  

MERGE INTO CustomerAddress
   USING (
          SELECT addressId, customerId 
            FROM CustomerAddressRelation           
         ) AS source
      ON CustomerAddress.addressId = source.addressId
WHEN MATCHED THEN
   UPDATE 
      SET CustomerAddress.CustomerId=source.customerId;

go

drop table DirectorAddressRelation
drop table CustomerAddressRelation
delete from CustomerAddress where customerId is null and directorId is null