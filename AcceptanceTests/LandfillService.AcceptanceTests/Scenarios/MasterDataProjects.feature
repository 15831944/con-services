﻿Feature: MasterDataProjects

Scenario: Create a new landfill project 
Given I inject the following master data events
| Event              | DaysToExpire | Boundaries | ProjectName | Type     | TimeZone                  |
| CreateProjectEvent | 10           | boundary   | AcceptTest  | LandFill | New Zealand Standard Time |
When I request a list of projects from landfill web api 
Then I find the project I created in the list  


@Sanity @Positive
@MasterDataProjects
Scenario: Update a new landfill project 
Given I inject the following master data events
| Event              | DaysToExpire | Boundaries | ProjectName  | Type     | TimeZone        |
| CreateProjectEvent | 10           | boundary   | AcceptUpdate | LandFill | America/Chicago |
And I inject the following master data events
| Event              | DaysToExpire | ProjectName      |  Type     |
| UpdateProjectEvent | 50           | AcceptanceUpdate |  Full3D   |
When I request a list of projects from landfill web api 
Then I find update project details in the project list  

@Sanity @Positive
@MasterDataProjects
Scenario: Delete a new landfill project 
Given I inject the following master data events
| Event              | DaysToExpire | Boundaries | ProjectName  | Type     | TimeZone        |
| CreateProjectEvent | 10           | boundary   | AcceptDelete | LandFill | America/Chicago |
And I inject the following master data events
| Event              | 
| DeleteProjectEvent |
When I request a list of projects from landfill web api 
Then I dont find the project I created in the list  