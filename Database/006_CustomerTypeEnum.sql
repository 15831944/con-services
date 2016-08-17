

CREATE TABLE IF NOT EXISTS CustomerTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(20) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_CustomerTypeEnum (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE CustomerTypeEnum
  (ID,Description) VALUES (0, 'Dealer');
INSERT IGNORE CustomerTypeEnum
  (ID,Description) VALUES (1, 'Customer');


-- there will be more, but not sure what format nextGen customer/orgs etc will take.
