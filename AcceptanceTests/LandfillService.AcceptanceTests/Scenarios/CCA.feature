﻿Feature: CCA

Scenario: CCA Ratio - whole project
	Given I have a landfill project 'Maddington' with landfill sites 'MarylandsLandfill,AmiStadiumLandfill'
		And I have the following machines
		| Name  | IsJohnDoe |
		| Cat A | false     |
		| Cat B | true      |
		And I have the following CCA data
		| Project    | Site       | DateAsAnOffsetFromToday | Machine | LiftID | Incomplete | Complete | Overcomplete |
		| Maddington | Marylands  | -2                      | Cat A   | null   | 20         | 35       | 45           |
		| Maddington | Marylands  | -1                      | Cat B   | 1      | 15         | 5        | 80           |
		| Maddington | AmiStadium | -2                      | Cat A   | null   | 35         | 15       | 50           |
		| Maddington | AmiStadium | -1                      | Cat B   | 1      | 10         | 60       | 30           |
		| Maddington | Maddington | -2                      | Cat A   | null   | 65         | 20       | 15           |
		| Maddington | Maddington | -1                      | Cat B   | 1      | 25         | 30       | 45           |
	When I request CCA ratio for site 'Maddington' for the past 2 days
	Then the response contains the following CCA ration data
	| Machine | DateAsAnOffsetFromToday | CCARatio |
	| Cat A   | -2                      | 35       |
	| Cat A   | -1                      | 0        |

Scenario: CCA Ratio - Individual site
	Given I have a landfill project 'Maddington' with landfill sites 'MarylandsLandfill,AmiStadiumLandfill'
		And I have the following machines
		| Name  | IsJohnDoe |
		| Cat A | false     |
		| Cat B | true      |
		And I have the following CCA data
		| Project    | Site       | DateAsAnOffsetFromToday | Machine | LiftID | Incomplete | Complete | Overcomplete |
		| Maddington | Marylands  | -2                      | Cat A   | null   | 20         | 35       | 45           |
		| Maddington | Marylands  | -1                      | Cat B   | 1      | 15         | 5        | 80           |
		| Maddington | AmiStadium | -2                      | Cat A   | null   | 35         | 15       | 50           |
		| Maddington | AmiStadium | -1                      | Cat B   | 1      | 10         | 60       | 30           |
		| Maddington | Maddington | -2                      | Cat A   | null   | 65         | 20       | 15           |
		| Maddington | Maddington | -1                      | Cat B   | 1      | 25         | 30       | 45           |
	When I request CCA ratio for site 'Marylands' for the past 2 days
	Then the response contains the following CCA ration data
	| Machine | DateAsAnOffsetFromToday | CCARatio |
	| Cat A   | -2                      | 80       |
	| Cat A   | -1                      | 0        |

Scenario: CCA Summary - whole project all lifts all machines
	Given I have a landfill project 'Maddington' with landfill sites 'MarylandsLandfill,AmiStadiumLandfill'
		And I have the following machines
		| Name  | IsJohnDoe |
		| Cat A | false     |
		| Cat B | true      |
		| Cat C | true      |
		And I have the following CCA data
		| Project    | Site       | DateAsAnOffsetFromToday | Machine | LiftID | Incomplete | Complete | Overcomplete |
		| Maddington | Marylands  | -1                      | Cat A   | null   | 20         | 35       | 45           |
		| Maddington | Marylands  | -1                      | Cat B   | 1      | 15         | 5        | 80           |
		| Maddington | AmiStadium | -1                      | Cat A   | null   | 35         | 15       | 50           |
		| Maddington | AmiStadium | -1                      | Cat B   | 1      | 10         | 60       | 30           |
		| Maddington | Maddington | -1                      | Cat A   | null   | 16         | 53       | 31           |
		| Maddington | Maddington | -1                      | Cat B   | 1      | 25         | 30       | 45           |
		| Maddington | Maddington | -1                      | Cat C   | null   | 72         | 16       | 12           |
	When I request CCA summary for lift 'AllLifts' of 'AllJohnDoes' machine 'AllMachines' in site 'Maddington' for day -1
	Then the response contains the following CCA summary data
	| Machine | LiftID | Incomplete | Complete | Overcomplete |
	| Cat A   | null   | 16         | 53       | 31           |
	| Cat C   | null   | 72         | 16       | 12           |