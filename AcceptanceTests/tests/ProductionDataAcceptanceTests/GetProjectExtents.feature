﻿Feature: GetProjectExtents
	I should be able to get project extents.

Background: 
	Given the Project Extent service URI "/api/v1/projectextents"

@requireDummySurveyedSurface
Scenario: GetProjectExtents - Excluding Surveyed Surfaces
	Given a GetProjectExtents project id 1001158
		And I decide to exclude any surveyed surface
	When I try to get the extents
	Then the following Bounding Box ThreeD Grid values should be returned
		| maxX               | maxY    | maxZ              | minX    | minY               | minZ             |
		| 2913.2900000000004 | 1250.69 | 624.1365966796875 | 2306.05 | 1125.2300000000002 | 591.953857421875 |

@requireDummySurveyedSurface
Scenario: GetProjectExtents - Including Surveyed Surfaces
	Given a GetProjectExtents project id 1001158
	When I try to get the extents
	Then the following Bounding Box ThreeD Grid values should be returned
		| maxX               | maxY    | maxZ              | minX | minY | minZ |
		| 2913.2900000000004 | 1250.69 | 624.1365966796875 | 0    | 0    | 0    |

Scenario: GetProjectExtents - Bad Request (Null Project ID)
	Given a GetProjectExtents null project id
	When I try to get the extents expecting badrequest
	Then I should get error code -4

Scenario: GetProjectExtents - Bad Request (Invalid Project ID)
	Given a GetProjectExtents project id 0
	When I try to get the extents expecting badrequest
	Then I should get error code -2

Scenario: GetProjectExtents - Bad Request (Empty Request)
	When I post an empty request
	Then I should get error code -3

Scenario: GetProjectExtents - Bad Request (Deleted Project)
	Given a GetProjectExtents project id 1000992
	When I try to get the extents expecting badrequest
	Then I should get error code -4
