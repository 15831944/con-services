USE `Alpha-Project-ccss`;

CREATE TABLE IF NOT EXISTS ProjectTypeEnum (
  ID int(11) NOT NULL,
  Description varchar(20) NOT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_ProjectTypeEnum_ID (ID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

INSERT IGNORE ProjectTypeEnum
  (ID,Description) VALUES (0, 'Standard');  


