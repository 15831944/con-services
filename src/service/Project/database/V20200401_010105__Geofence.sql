
CREATE TABLE IF NOT EXISTS Geofence (
  ID bigint(20) NOT NULL AUTO_INCREMENT,
  GeofenceUID varchar(100) NOT NULL,
  Name varchar(128) NOT NULL,
  fk_GeofenceTypeID int(11) NOT NULL,
  PolygonST polygon DEFAULT NULL,
  FillColor int(11) NOT NULL,
  IsTransparent tinyint(1) NOT NULL,
  IsDeleted tinyint(1) DEFAULT '0',
  Description varchar(36) DEFAULT NULL,
  AreaSqMeters decimal(10,0) DEFAULT '0',
  fk_CustomerUID varchar(100) NOT NULL,
  UserUID varchar(100) NOT NULL,
  LastActionedUTC datetime(6) DEFAULT NULL,
  InsertUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
  UpdateUTC datetime(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6) ON UPDATE CURRENT_TIMESTAMP(6),
  PRIMARY KEY (ID),
  UNIQUE KEY UIX_Geofence_GeofenceUID (GeofenceUID)
) ENGINE=InnoDB CHARSET = DEFAULT COLLATE = DEFAULT;
