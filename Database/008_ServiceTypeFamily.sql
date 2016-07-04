  
  CREATE TABLE IF NOT EXISTS ServiceTypeFamilyEnum (
  ID int(11) NOT NULL,
  Description varchar(20) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_ServiceTypeFamilyEnum (ID)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

INSERT IGNORE ServiceTypeFamilyEnum
  (ID,Description) VALUES (1, 'Asset');
INSERT IGNORE ServiceTypeFamilyEnum
  (ID,Description) VALUES (2, 'Customer');
INSERT IGNORE ServiceTypeFamilyEnum
  (ID,Description) VALUES (3, 'Project');  


