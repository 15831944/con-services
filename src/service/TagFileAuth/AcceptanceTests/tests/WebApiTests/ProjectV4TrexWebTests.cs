﻿using System;
using System.Collections.Generic;
using VSS.Productivity3D.TagFileAuth.Models;
using VSS.Productivity3D.TagFileAuth.Models.ResultsHandling;
using VSS.Productivity3D.TagFileAuth.WebAPI.Models.Enums;
using Xunit;

namespace WebApiTests
{
  public class ProjectV4TrexWebTests : ExecutorTestData
  {
    private const string ValidTPaaSUserJWT =
      "eyJhbGciOiJSUzI1NiIsIng1dCI6IlltRTNNelE0TVRZNE5EVTJaRFptT0RkbU5UUm1OMlpsWVRrd01XRXpZbU5qTVRrek1ERXpaZyJ9.eyJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9hcHBsaWNhdGlvbm5hbWUiOiJBbHBoYS1WTFVuaWZpZWRGbGVldCIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL3Bhc3N3b3JkUG9saWN5RGV0YWlscyI6ImV5SjFjR1JoZEdWa1ZHbHRaU0k2TVRRM056VXlOalExT1RjM09Td2lhR2x6ZEc5eWVTSTZXeUppTURSa05tWmxZVFZtWW1GbVpURXdabVU1WkRCa016RTFNalU0TVRsbU1HUTNNMkZoWXpsbVptWTBZbVE1T0RFMk5HTmlNRGd4WVRZMU56SXhOamxpSWl3aU1tRmhaVFl3WldSak1XSmhOREUxWXpCaFlqQTVNMll3TmpWbFlqWXhORGc0TTJJd05HSmtNV0poWm1ZNU9HWXdZbU5sWVRVNU5HTmlZbVl6T1RSa09DSmRmUT09IiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wva2V5dHlwZSI6IlBST0RVQ1RJT04iLCJzY29wZXMiOiJvcGVuaWQiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9lbWFpbFZlcmlmaWVkIjoidHJ1ZSIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL3N1YnNjcmliZXIiOiJkZXYtdnNzYWRtaW5AdHJpbWJsZS5jb20iLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC91c2VydHlwZSI6IkFQUExJQ0FUSU9OX1VTRVIiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9yb2xlIjoiIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvYWNjb3VudHVzZXJuYW1lIjoiY2xheWFuZGVyc29uYXR0cmltYmxlK2NhdGRlbW8iLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9hY2NvdW50bmFtZSI6ImdtYWlsLmNvbSIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2ZpcnN0bmFtZSI6IkNsYXkiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9wYXNzd29yZFBvbGljeSI6IkhJR0giLCJpc3MiOiJ3c28yLm9yZ1wvcHJvZHVjdHNcL2FtIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvbGFzdG5hbWUiOiJBbmRlcnNvbiIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2FwcGxpY2F0aW9uaWQiOiIzNDAyIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvdmVyc2lvbiI6IjEuMCIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2VuZHVzZXIiOiJnbWFpbC5jb20hY2xheWFuZGVyc29uYXR0cmltYmxlK2NhdGRlbW9AaW5kaXZpZHVhbC5jb20iLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC91dWlkIjoiY2IwZWJlOGEtYjk2MC00ZjVkLTg1NTItMDQzOWY1ZjBkZmU2IiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvZW5kdXNlclRlbmFudElkIjoiMiIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2dpdmVubmFtZSI6IkNsYXkiLCJleHAiOjE0OTE0OTcwMDUsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2lkZW50aXR5XC9mYWlsZWRMb2dpbkF0dGVtcHRzIjoiMCIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2lkZW50aXR5XC9hY2NvdW50TG9ja2VkIjoiZmFsc2UiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9hcGljb250ZXh0IjoiXC90XC90cmltYmxlLmNvbVwvVlNTLUFscGhhLVVuaWZpZWRGbGVldCIsImh0dHA6XC9cL3dzbzIub3JnXC9jbGFpbXNcL2xhc3RMb2dpblRpbWVTdGFtcCI6IjE0OTE0OTM4NjY3OTUiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC90aWVyIjoiVW5saW1pdGVkIiwiaHR0cDpcL1wvd3NvMi5vcmdcL2NsYWltc1wvc3RhdHVzIjoiZXlKQ1RFOURTMFZFSWpvaVptRnNjMlVpTENKWFFVbFVTVTVIWDBaUFVsOUZUVUZKVEY5V1JWSkpSa2xEUVZSSlQwNGlPaUptWVd4elpTSXNJa0pTVlZSRlgwWlBVa05GWDB4UFEwdEZSQ0k2SW1aaGJITmxJaXdpUVVOVVNWWkZJam9pZEhKMVpTSjkiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9hcHBsaWNhdGlvbnRpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOlwvXC93c28yLm9yZ1wvY2xhaW1zXC9lbWFpbGFkZHJlc3MiOiJjbGF5YW5kZXJzb25hdHRyaW1ibGUrY2F0ZGVtb0BnbWFpbC5jb20ifQ.JRwGoInSn4Ohs-9XX-p_y76zzUFVN9xfNt9W3fH7Up9XxMJVs_wqu7bPZVYEAssTnOjrGY7pE-7EsX-DS_pEAwunmTAQzHlciFtX8XLpQfTEBcd6UuuJdbH7zXCVyqeJH1OzyZ6xddzKzBwjKZR0JBA-O5fQzORXkmc5_IuYPVw";

    private const string ValidTPaaSApplicationJWT =
      "eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsIng1dCI6IlltRTNNelE0TVRZNE5EVTJaRFptT0RkbU5UUm1OMlpsWVRrd01XRXpZbU5qTVRrek1ERXpaZz09In0=.eyJpc3MiOiJ3c28yLm9yZy9wcm9kdWN0cy9hbSIsImV4cCI6MTQ4Njc3MjA3NDE5MSwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9zdWJzY3JpYmVyIjoicHViLXZzc2FkbWluQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbmlkIjoiNjYyIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcHBsaWNhdGlvbm5hbWUiOiJNYXN0ZXJEYXRhTWFuYWdlbWVudCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvYXBwbGljYXRpb250aWVyIjoiVW5saW1pdGVkIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hcGljb250ZXh0IjoiL3QvdHJpbWJsZS5jb20vdnNzLWlxYS1hc3NldHNlcnZpY2UiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3ZlcnNpb24iOiIxLjAiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3RpZXIiOiJVbmxpbWl0ZWQiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2tleXR5cGUiOiJQUk9EVUNUSU9OIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91c2VydHlwZSI6IkFQUExJQ0FUSU9OIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyIjoicHViLXZzc2FkbWluQHRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9lbmR1c2VyVGVuYW50SWQiOiIxIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hY2NvdW50bmFtZSI6InRyaW1ibGUuY29tIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9hY2NvdW50dXNlcm5hbWUiOiJwdWItdnNzYWRtaW4iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2VtYWlsYWRkcmVzcyI6IkJob29iYWxhbl9QYWxhbml2ZWxAVHJpbWJsZS5jb20iLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2ZpcnN0bmFtZSI6IkJob29iYWxhbiIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvZ2l2ZW5uYW1lIjoiQmhvb2JhbGFuIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS9hY2NvdW50TG9ja2VkIjoiZmFsc2UiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL2lkZW50aXR5L2ZhaWxlZExvZ2luQXR0ZW1wdHMiOiIwIiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy9pZGVudGl0eS91bmxvY2tUaW1lIjoiMCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdExvZ2luVGltZVN0YW1wIjoiMTQ4NjczMzU2NjgxNCIsImh0dHA6Ly93c28yLm9yZy9jbGFpbXMvbGFzdG5hbWUiOiJQYWxhbml2ZWwiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3JvbGUiOiJwdWJsaXNoZXIiLCJodHRwOi8vd3NvMi5vcmcvY2xhaW1zL3N0YXR1cyI6ImV5SkNURTlEUzBWRUlqb2labUZzYzJVaUxDSlhRVWxVU1U1SFgwWlBVbDlGVFVGSlRGOVdSVkpKUmtsRFFWUkpUMDRpT2lKbVlXeHpaU0lzSWtKU1ZWUkZYMFpQVWtORlgweFBRMHRGUkNJNkltWmhiSE5sSWl3aVFVTlVTVlpGSWpvaWRISjFaU0o5IiwiaHR0cDovL3dzbzIub3JnL2NsYWltcy91dWlkIjoiMzUzOGNlZTItNTdiZS00YzA5LTgwODYtNTkyZjBlMzRmYzEzIn0=.De+fBh9VlvBFTy+NCcd3qUCLEqQH4vO3iJy1SWTFOT7JhR+z8eVC+wM70nVZttsPpmKh8IbI2FICnAc6i25DxeLEsREtUOzkUffdAEaQXEEv6Up0JA1YlPkOMIl3g74e3XcRMuKizZ7m4cllpj2ooJqrrdc3OvIDV/fUWlHJ6MI=";


    protected Dictionary<string, string> _customHeaders;

    public ProjectV4TrexWebTests()
    {
      _customHeaders = new Dictionary<string, string>()
      {
        { "X-JWT-Assertion", ValidTPaaSApplicationJWT }
      };
    }

    [Fact(Skip = "todoMaverick Temporary ignore until we get TFA authenication key generated.")]
    public async System.Threading.Tasks.Task Manual_Sad_ProjectNotFound()
    {
      // this test can be made to work through TFA service, through to ProjectSvc - if you setup environment variables appropriately
      var projectUid = Guid.NewGuid().ToString();
      var CBRadioType = TagFileDeviceTypeEnum.SNM940;
      var CBRadioserial = dimensionsRadioSerial;
      var EC50Serial = string.Empty;
      double latitude = 89;
      double longitude = 130;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(projectUid, (int)CBRadioType, CBRadioserial,
        EC50Serial, latitude, longitude, tagFileTimestamp);
      getProjectAndAssetUidsRequest.Validate();
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);

      var result = await tagFileAuthProjectProxy.GetProjectAndAssetUids(getProjectAndAssetUidsRequest, _customHeaders);

      ValidateResult(result, expectedGetProjectAndAssetUidsResult, 3038, "Unable to find the Project requested");
    }

    [Fact(Skip = "todoMaverick Temporary ignore until we get TFA authenication key generated.")]
    public async System.Threading.Tasks.Task Auto_Sad_DeviceNotFound()
    {
      // this test can be made to work through TFA service, through to ProjectSvc - if you setup environment variables appropriately
      var CBRadioType = TagFileDeviceTypeEnum.SNM940;
      var CBRadioserial = Guid.NewGuid().ToString();
      var EC50Serial = string.Empty;
      double latitude = 89;
      double longitude = 130;
      var tagFileTimestamp = DateTime.UtcNow.AddDays(-10);

      var getProjectAndAssetUidsRequest = new GetProjectAndAssetUidsRequest(string.Empty, (int)CBRadioType, CBRadioserial,
        EC50Serial, latitude, longitude, tagFileTimestamp);
      getProjectAndAssetUidsRequest.Validate();
      var expectedGetProjectAndAssetUidsResult = new GetProjectAndAssetUidsResult(string.Empty, string.Empty);

      var result = await tagFileAuthProjectProxy.GetProjectAndAssetUids(getProjectAndAssetUidsRequest, _customHeaders);

      ValidateResult(result, expectedGetProjectAndAssetUidsResult, 3047, "Auto Import: unable to identify the device by this serialNumber");
    }

    private void ValidateResult(GetProjectAndAssetUidsResult actualResult, GetProjectAndAssetUidsResult expectedGetProjectAndAssetUidsResult,
      int resultCode, string resultMessage)
    {
      Assert.NotNull(actualResult);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.ProjectUid, actualResult.ProjectUid);
      Assert.Equal(expectedGetProjectAndAssetUidsResult.DeviceUid, actualResult.DeviceUid);
      Assert.Equal(resultCode, actualResult.Code);
      Assert.Equal(resultMessage, actualResult.Message);
    }
  }
}

