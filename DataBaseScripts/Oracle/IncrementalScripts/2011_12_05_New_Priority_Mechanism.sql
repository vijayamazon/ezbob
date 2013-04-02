execute immediate 'alter TABLE Signal ADD OwnerApplicationId NUMBER NULL';
execute immediate 'UPDATE Signal SET OwnerApplicationId = Priority';
execute immediate 'UPDATE Signal SET Priority = 0';