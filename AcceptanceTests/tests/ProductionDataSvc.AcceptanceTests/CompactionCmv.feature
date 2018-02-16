﻿# (Aaron) Ignored at Raymond's request.
@IgnoreAll
Feature: CompactionCmv
I should be able to request compaction CMV data

######################################################## CMV Summary ############################################################
Scenario Outline: Compaction Get CMV Summary - No Design Filter
Given the Compaction service URI "/api/v2/cmv/summary" for operation "CMVSummary"
And the result file "CompactionGetCMVDataResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName     | ProjectUID                           | ResultName                |
|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Summary    |
| ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Summary_PS |

Scenario Outline: Compaction Get CMV Summary
Given the Compaction service URI "/api/v2/cmv/summary" for operation "CMVSummary"
And the result file "CompactionGetCMVDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName      | ProjectUID                           | FilterUID                            | ResultName               |
| DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Summary    |
| DesignIntersects | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Summary |
| FilterArea       | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | BoundaryFilter_Summary   |


######################################################## CMV Details ############################################################
Scenario Outline: Compaction Get CMV Details - No Design Filter
Given the Compaction service URI "/api/v2/cmv/details" for operation "CMVDetails"
And the result file "CompactionGetCMVDataResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName     | ProjectUID                           | ResultName                |
|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_Details    |
| ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_Details_PS |

Scenario Outline: Compaction Get CMV Details
Given the Compaction service URI "/api/v2/cmv/details" for operation "CMVDetails"
And the result file "CompactionGetCMVDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName      | ProjectUID                           | FilterUID                            | ResultName               |
| DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_Details    |
| DesignIntersects | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_Details |


######################################################## CMV % Change Summary ###################################################
Scenario Outline: Compaction Get CMV % Change Summary - No Design Filter
Given the Compaction service URI "/api/v2/cmv/percentchange" for operation "CMVPercentChangeSummary"
And the result file "CompactionGetCMVDataResponse.json"
And projectUid "<ProjectUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName     | ProjectUID                           | ResultName                      |
|                 | ff91dd40-1569-4765-a2bc-014321f76ace | NoDesignFilter_PercentChange    |
| ProjectSettings | 3335311a-f0e2-4dbe-8acd-f21135bafee4 | NoDesignFilter_PercentChange_PS |

Scenario Outline: Compaction Get CMV % Change Summary
Given the Compaction service URI "/api/v2/cmv/percentchange" for operation "CMVPercentChangeSummary"
And the result file "CompactionGetCMVDataResponse.json"
And projectUid "<ProjectUID>"
And filterUid "<FilterUID>"
When I request result
Then the result should match the "<ResultName>" from the repository
Examples: 
| RequestName      | ProjectUID                           | FilterUID                            | ResultName                            |
| DesignOutside    | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 1cf81668-1739-42d5-b068-ea025588796a | DesignOutside_PercentChangeSummary    |
| DesignIntersects | 7925f179-013d-4aaf-aff4-7b9833bb06d6 | 3d9086f2-3c04-4d92-9141-5134932b1523 | DesignIntersects_PercentChangeSummary |
| FilterArea       | ff91dd40-1569-4765-a2bc-014321f76ace | a37f3008-65e5-44a8-b406-9a078ec62ece | BoundaryFilter_PercentChangeSummary   |
