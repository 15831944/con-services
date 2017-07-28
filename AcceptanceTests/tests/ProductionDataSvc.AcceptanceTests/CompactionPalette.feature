﻿Feature: CompactionPalette
  I should be able to request compaction palettes

 Scenario: Compaction Get Elevation Palette When No Elevation Data
	Given the Compaction Elevation Palette service URI "/api/v2/compaction/elevationpalette"
  And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
  And a startUtc "2017-01-01" and an EndUtc "2017-01-01"
	When I request Elevation Palette
	Then the Elevation Palette result should be
  """
  {
    "palette": null,
     "Code": -4,
    "Message": "No elevation range"
  }
	"""

  Scenario: Compaction Get Elevation Palette 
	Given the Compaction Elevation Palette service URI "/api/v2/compaction/elevationpalette"
  And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Elevation Palette
	Then the Elevation Palette result should be
  """
  {
    "palette": {
      "colorValues": [
        {
          "color": 13107200,
          "value": 591.9539794921875
        },
        {
          "color": 16711680,
          "value": 593.02544759114585
        },
        {
          "color": 14760960,
          "value": 594.0969156901042
        },
        {
          "color": 16734720,
          "value": 595.16838378906255
        },
        {
          "color": 16744960,
          "value": 596.23985188802078
        },
        {
          "color": 16755200,
          "value": 597.31131998697913
        },
        {
          "color": 16762880,
          "value": 598.38278808593748
        },
        {
          "color": 16768000,
          "value": 599.45425618489583
        },
        {
          "color": 16442880,
          "value": 600.52572428385417
        },
        {
          "color": 14476800,
          "value": 601.59719238281252
        },
        {
          "color": 13821440,
          "value": 602.66866048177087
        },
        {
          "color": 13166080,
          "value": 603.74012858072922
        },
        {
          "color": 11855360,
          "value": 604.81159667968745
        },
        {
          "color": 9889280,
          "value": 605.8830647786458
        },
        {
          "color": 8578560,
          "value": 606.95453287760415
        },
        {
          "color": 6615040,
          "value": 608.0260009765625
        },
        {
          "color": 65280,
          "value": 609.09746907552085
        },
        {
          "color": 61540,
          "value": 610.1689371744792
        },
        {
          "color": 59010,
          "value": 611.24040527343755
        },
        {
          "color": 59030,
          "value": 612.31187337239578
        },
        {
          "color": 59060,
          "value": 613.38334147135413
        },
        {
          "color": 59080,
          "value": 614.45480957031248
        },
        {
          "color": 59090,
          "value": 615.52627766927083
        },
        {
          "color": 56540,
          "value": 616.59774576822917
        },
        {
          "color": 51430,
          "value": 617.66921386718752
        },
        {
          "color": 46320,
          "value": 618.74068196614587
        },
        {
          "color": 38645,
          "value": 619.81215006510422
        },
        {
          "color": 30970,
          "value": 620.88361816406245
        },
        {
          "color": 23295,
          "value": 621.9550862630208
        },
        {
          "color": 18175,
          "value": 623.02655436197915
        },
        {
          "color": 255,
          "value": 624.0980224609375
        }
      ],
      "aboveLastColor": 8388736,
      "belowFirstColor": 16711935
    },
    "Code": 0,
    "Message": "success"
  }
	"""
   
  Scenario: Compaction Get Palettes 
	Given the Compaction Palettes service URI "/api/v2/compaction/colorpalettes"
  And a projectUid "ff91dd40-1569-4765-a2bc-014321f76ace"
	When I request Palettes
	Then the Palettes result should be
  """
  {
    "cmvDetailPalette": {
      "colorValues": [
        {
        "color": 2971523,
        "value": 0.0
				},
				{
					"color": 4430812,
					"value": 10.0
				},
				{
					"color": 12509169,
					"value": 20.0
				},
				{
					"color": 10341991,
					"value": 30.0
				},
				{
					"color": 7053374,
					"value": 40.0
				},
				{
					"color": 3828517,
					"value": 50.0
				},
				{
					"color": 16174803,
					"value": 60.0
				},
				{
					"color": 13990524,
					"value": 70.0
				},
				{
					"color": 12660791,
					"value": 80.0
				},
				{
					"color": 15105570,
					"value": 90.0
				},
				{
					"color": 14785888,
					"value": 100.0
				},
				{
					"color": 15190446,
					"value": 110.0
				},
				{
					"color": 5182823,
					"value": 120.0
				},
				{
					"color": 9259433,
					"value": 130.0
				},
				{
					"color": 13740258,
					"value": 140.0
				},
				{
					"color": 1971179,
					"value": 150.0
				}
      ],
      "aboveLastColor": null,
      "belowFirstColor": null
    },
    "passCountDetailPalette": {
      "colorValues": [
        {
          "color": 2971523,
          "value": 1
        },
        {
          "color": 4430812,
          "value": 2
        },
        {
          "color": 12509169,
          "value": 3
        },
        {
          "color": 10341991,
          "value": 4
        },
        {
          "color": 7053374,
          "value": 5
        },
        {
          "color": 3828517,
          "value": 6
        },
        {
          "color": 16174803,
          "value": 7
        },
        {
          "color": 13990524,
          "value": 8
        }
      ],
      "aboveLastColor": 12660791,
      "belowFirstColor": null
    },
    "passCountSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "cutFillPalette": {
      "colorValues": [
        {
          "color": 11789820,
          "value": -0.2
        },
        {
          "color": 236517,
          "value": -0.1
        },
        {
          "color": 87963,
          "value": -0.05
        },
        {
          "color": 9159498,
          "value": 0
        },
        {
          "color": 16764370,
          "value": 0.05
        },
        {
          "color": 15037299,
          "value": 0.1
        },
        {
          "color": 13959168,
          "value": 0.2
        }
      ],
      "aboveLastColor": null,
      "belowFirstColor": null
    },
    "temperatureSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "cmvSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "mdpSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "cmvPercentChangePalette": {
      "colorValues": [
        {
          "color": 9159498,
          "value": 5
        },
        {
          "color": 16764370,
          "value": 20
        },
        {
          "color": 15037299,
          "value": 50
        }
      ],
      "aboveLastColor": 13959168,
      "belowFirstColor": 33554431
    },
    "speedSummaryPalette": {
      "aboveTargetColor": 13959168,
      "onTargetColor": 9159498,
      "belowTargetColor": 87963
    },
    "temperatureDetailPalette": {
      "colorValues": [
        {
          "color": 2971523,
          "value": 70
        },
        {
          "color": 4430812,
          "value": 80
        },
        {
          "color": 12509169,
          "value": 90
        },
        {
          "color": 14479047,
          "value": 100
        },
        {
          "color": 10341991,
          "value": 110
        },
        {
          "color": 7053374,
          "value": 120
        },
        {
          "color": 3828517,
          "value": 130
        },
        {
          "color": 16174803,
          "value": 140
        },
        {
          "color": 13990524,
          "value": 150
        },
        {
          "color": 12660791,
          "value": 160
        }
      ],
      "aboveLastColor": null,
      "belowFirstColor": null
    },
    "Code": 0,
    "Message": "success"
  }
	"""
