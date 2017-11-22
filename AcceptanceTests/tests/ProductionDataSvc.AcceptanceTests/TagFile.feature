﻿Feature: TagFile
	I should be able to POST tag files.

Background: 
	Given the Tag file service URI "/api/v1/tagfiles" and request repo "TagFileRequest.json"

Scenario: TagFile - Good Request
	When I POST a tag file with code 100 from the repository
	Then the Tag Process Service response should contain Code 0 and Message "success"

Scenario Outline: TagFile - Bad Tag File
	When I POST a tag file with Code <Code> from the repository expecting bad request return
	Then the Tag Process Service response should contain Code <Code> and Message <Message>
	Examples: 
	| Code | Message                                                                                                                     |
	| 2005 | "Failed to process tagfile with error: The TAG file was found to be corrupted on its pre-processing scan."                  |
	| 2008 | "Failed to process tagfile with error: OnChooseMachine. Machine Subscriptions Invalid."                                     |
	| 2014 | "Failed to process tagfile with error: OnChooseDataModel. First Epoch Blade Position Does Not Lie Within Project Boundary." |

Scenario Outline: TagFile - Bad Request
	When I POST a Tag file with name "<paramName>" from the repository expecting bad request return
	Then the Tag Process Service response should contain Error Code <code>
	Examples: 
	| paramName        | code |
	| NullFileName     | -1   |
	| NullData         | -1   |
	| NullBoundary     | -1   |
#this is valid - machine ID can be null if not overriden
#| NullMachineId    | 2008 |
	| InvalidProjectId | -1   |
	| FilenameTooLong  | -1   |