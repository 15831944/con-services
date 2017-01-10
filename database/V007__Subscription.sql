

CREATE TABLE IF NOT EXISTS Subscription (
  ID BIGINT(20) NOT NULL AUTO_INCREMENT,
  SubscriptionUID varchar(36) NOT NULL,
  fk_CustomerUID varchar(36) NOT NULL,
  fk_ServiceTypeID INT(11)  NOT NULL,
  StartDate date DEFAULT NULL,
  EndDate date DEFAULT NULL,
  EffectiveDate date DEFAULT NULL,
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),  
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_Subscription_SubscriptionUID (SubscriptionUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;

