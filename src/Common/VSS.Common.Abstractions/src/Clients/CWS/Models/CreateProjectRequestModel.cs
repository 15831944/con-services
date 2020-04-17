﻿using Newtonsoft.Json;

namespace VSS.Common.Abstractions.Clients.CWS.Models
{
  // CCSSSCON-23 this doesn't include start and end dates or description
  //  marketing are still going back and forth on these
  public class CreateProjectRequestModel
  {
    /// <summary>
    /// Account TRN ID
    /// </summary>
    [JsonProperty("accountId")]
    public string accountId { get; set; }

    /// <summary>
    /// Project name
    /// </summary>
    [JsonProperty("projectName")]
    public string projectName { get; set; }

    /// <summary>
    /// cws example = "America/Denver"
    ///    this field is optional. We should never set it.
    ///    cws has its own set which it uses internally, and calculates off the boundary
    ///    3dp has a different set, so we'll just use our own
    /// </summary>
    //[JsonProperty("timezone")]
    //public string timezone { get; set; }

    /// <summary>
    /// 3dp supports what types?
    /// </summary>
    [JsonProperty("boundary")]
    public ProjectBoundary boundary { get; set; }

  }

  /* example
    {
      "accountId": "{{accountId}}",
      "projectName": "{{projectName}}",
      "timezone": "America/Denver",
      "boundary": {
        "type": "Polygon",
        "coordinates": [
            [
                [
                    -105.115560734865,
                    39.898797315920504
                ],
                [
                    -105.11758904248114,
                    39.89642626198623
                ],
                [
                    -105.1139381054785,
                    39.894661780621746
                ],
                [
                    -105.11212447489478,
                    39.8973452453979
                ],
                [
                    -105.115560734865,
                    39.898797315920504
                ]
            ]
        ]
    }
  */
}
