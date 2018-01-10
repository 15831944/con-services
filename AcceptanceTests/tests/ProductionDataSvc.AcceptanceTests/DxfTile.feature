﻿Feature: DxfTile
	I should be able to request DXF tiles

Scenario: Dxf Tile - Good Request 
	Given the Dxf Tile service URI "/api/v2/lineworktiles" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a bbox "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625" and a width "256" and a height "256"
  And a fileType "linework"
	When I request a Dxf Tile
	Then the Dxf Tile result image should be match within "1" percent
  """
  {
    "tileData": "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAYAAABccqhmAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAFySURBVHhe7dRBCgQhDARAv+v/H7CjMkIIwsAeYxWI2nrtNAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAIC/9d5/c71X4Ba7+IYAXMoQgMvkou/yGwBQWCz46WwIQFGx5CsYTue5xxwoIpY87lvM8xtQQC53Lvq+539AAbHgKxhy0U9/gCK+hkB8jzlQRCz5CobTee4xv0drD/arc8l8x/8cAAAAAElFTkSuQmCC",
    "tileOutsideProjectExtents": false,
    "Code": 0,
    "Message": "success"
  }
	"""

Scenario: Dxf Tile - Scaled Tile 
	Given the Dxf Tile service URI "/api/v2/lineworktiles" 
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a bbox "-43.545561990655841, 172.58285522460938, -43.545064289564273, 172.58354187011719" and a width "256" and a height "256"
  And a fileType "linework"
	When I request a Dxf Tile
	Then the Dxf Tile result image should be match within "1" percent
  """
  {
    "tileData": "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",
    "tileOutsideProjectExtents": false,
    "Code": 0,
    "Message": "success"
  }
	"""

Scenario: Dxf Tile - No Imported Files 
	Given the Dxf Tile service URI "/api/v2/lineworktiles"
	And a projectUid "0fa94210-0d7a-4015-9eee-4d9956f4b250"
  And a bbox "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625" and a width "256" and a height "256"
  And a fileType "linework"
	When I request a Dxf Tile
  Then the Dxf Tile result should be
  """
  {
    "tileData": "iVBORw0KGgoAAAANSUhEUgAAAQAAAAEACAYAAABccqhmAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAEVSURBVHhe7cExAQAAAMKg9U/tawggAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABODQE8AAH/Cno5AAAAAElFTkSuQmCC",
    "tileOutsideProjectExtents": false,
    "Code": 0,
    "Message": "success"
  }
	"""

Scenario: Dxf Tile - No FileType 
	Given the Dxf Tile service URI "/api/v2/lineworktiles"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a bbox "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625" and a width "256" and a height "256"
  And a fileType ""
	When I request a Dxf Tile Expecting BadRequest
	Then I should get error code -1 and message "Missing file type"

Scenario: Dxf Tile - Bad FileType 
	Given the Dxf Tile service URI "/api/v2/lineworktiles"
	And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a bbox "-43.5445665843636, 172.578735351563, -43.5405847948288, 172.584228515625" and a width "256" and a height "256"
  And a fileType "SurveyedSurface"
	When I request a Dxf Tile Expecting BadRequest
  Then I should get error code -1 and message "Unsupported file type SurveyedSurface"


 