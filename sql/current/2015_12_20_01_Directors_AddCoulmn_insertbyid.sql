ALTER TABLE Director
    ADD UserId INTEGER,
    FOREIGN KEY(UserId) REFERENCES Security_User(UserId);