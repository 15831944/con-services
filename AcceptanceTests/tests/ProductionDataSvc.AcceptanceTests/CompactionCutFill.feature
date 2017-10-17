﻿Feature: CompactionCutFill
  I should be able to request Cut-Fill compaction data

@ignore
Scenario Outline: Compaction Get Cut-Fill Details - No Design Filter
	Given the Compaction service URI "/api/v2/compaction/cutfill/details" for operation "CutFillDetails"
  And the result file "CompactionGetCutFillDataResponse.json"
	And projectUid "<ProjectUID>"
  And a cutfillDesignUid "<CutFillDesignUID>"
	When I request result
  Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName     | ProjectUID                           | CutFillDesignUID                     | ResultName                |
  |                 | ff91dd40-1569-4765-a2bc-014321f76ace | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | NoDesignFilter_Details    |
	| ProjectSettings | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | NoDesignFilter_Details_PS |

@ignore
Scenario Outline: Compaction Get Cut-Fill Details
  Given the Compaction service URI "/api/v2/compaction/cutfill/details" for operation "CutFillDetails"
  And the result file "CompactionGetCutFillDataResponse.json"
  And projectUid "<ProjectUID>"
  And a cutfillDesignUid "<CutFillDesignUID>"
	And filterUid "<FilterUID>"
	When I request result
	Then the result should match the "<ResultName>" from the repository
	Examples: 
	| RequestName      | ProjectUID                           | CutFillDesignUID                     | FilterUID                            | ResultName               |
  | DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | dd64fe2e-6f27-4a78-82a3-0c0e8a5e84ff | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Details    |
	| DesignIntersects | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 220e12e5-ce92-4645-8f01-1942a2d5a57f | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Details |
