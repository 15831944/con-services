SET @s = (SELECT IF(
    (SELECT `AUTO_INCREMENT`
			FROM  INFORMATION_SCHEMA.TABLES
			WHERE TABLE_SCHEMA = DATABASE()
			AND   TABLE_NAME   = 'ImportedFile'
		) >= 1000000,
    "SELECT 1",
    "ALTER TABLE `ImportedFile` AUTO_INCREMENT = 1000000"
));  

PREPARE stmt FROM @s;
EXECUTE stmt;
DEALLOCATE PREPARE stmt;  
 