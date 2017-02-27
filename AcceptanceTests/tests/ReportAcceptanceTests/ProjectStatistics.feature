﻿Feature: ProjectStatistics
	I should be able to request statistics for a project.

Background: 
	Given the Project Stats service URI "/api/v1/projects/statistics"

@requireDummySurveyedSurface
Scenario: ProjectStatistics - Excluding Surveyed Surfaces
	Given a Project Statistics project id 1001158
		And I decide to exclude all surveyed surfaces
	When I request the project statistics
	Then I should get the following project statistics
		| startTime               | endTime                 | cellSize | indexOriginOffset | maxX      | maxY    | maxZ     | minX    | minY      | minZ     |
		| 2012-10-30T00:12:09.109 | 2012-11-08T01:00:08.756 | 0.34     | 536870912         | 2913.2900 | 1250.69 | 624.1365 | 2306.05 | 1125.2300 | 591.9538 |

@requireDummySurveyedSurface
Scenario: ProjectStatistics - Including Surveyed Surfaces
	Given a Project Statistics project id 1001158
	When I request the project statistics
	Then I should get the following project statistics
		| startTime               | endTime                 | cellSize | indexOriginOffset | maxX      | maxY    | maxZ     | minX | minY | minZ |
		| 2012-10-30T00:12:09.109 | 2015-03-15T18:13:09.265 | 0.34     | 536870912         | 2913.2900 | 1250.69 | 624.1365 | 0    | 0    | 0    |

Scenario: ProjectStatistics - Bad Request (Invalid Project)
	Given a Project Statistics project id 0
	When I request the project statistics expecting BadRequest
	Then I should get error code -2
