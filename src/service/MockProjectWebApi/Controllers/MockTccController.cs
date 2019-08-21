﻿using System;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MockProjectWebApi.Controllers
{
  public class MockTccController : BaseController
  {
    public MockTccController(ILoggerFactory loggerFactory) : base(loggerFactory)
    { }

    [Route("tcc/PutFile")]
    [HttpPut]
    public dynamic PutFile([FromQuery] string ticket, [FromQuery]string filespaceid, [FromQuery]string path, [FromQuery]string filename, [FromQuery]bool replace, [FromQuery]bool commitUpload)
    {
      Logger.LogInformation($"PutFile: {Request.QueryString}");

      //TODO: do we need [FromBody] Stream contents?

      return new
      {
        success = true,
        entryId = Guid.NewGuid(),
        path = $"{path}/{filename}",
        md5hash = "f710d1d95f995fe165714b4a35563d50"
      };
    }

    [Route("tcc/GetFile")]
    [HttpGet]
    public Stream GetFile([FromQuery] string ticket, [FromQuery]string filespaceid, [FromQuery]string path)
    {
      Logger.LogInformation($"GetFile: {Request.QueryString}");

      byte[] buffer = { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0, 1, 2, 3 };
      return new MemoryStream(buffer);
    }

    [Route("tcc/GetFileSpaces")]
    [HttpGet]
    public dynamic GetFileSpaces([FromQuery] string ticket)
    {
      Logger.LogInformation($"GetFileSpaces: {Request.QueryString}");

      return new
      {
        success = true,
        filespaces = new[]
        {
          new
          {
            filespaceId = "5u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
            orgDisplayName = "the orgDisplayName",
            orgId = "u8472cda0-9f59-41c9-a5e2-e19f922f91d8",
            orgShortname = "the org sn",
            shortname = "the sn",
            title = "the orgTitle"
          }
        }
      };
    }

    [Route("tcc/Copy")]
    [HttpGet]
    public dynamic Copy([FromQuery] string ticket, [FromQuery]string filespaceid, [FromQuery]string path, [FromQuery]string newfilespaceid, [FromQuery]string newpath, [FromQuery]bool merge, [FromQuery]bool replace)
    {
      Logger.LogInformation($"Copy: {Request.QueryString}");

      return new
      {
        success = true,
      };
    }

    [Route("tcc/GetFileAttributes")]
    [HttpGet]
    public dynamic GetFileAttributes([FromQuery] string ticket, [FromQuery]string filespaceid, [FromQuery]string path)
    {
      Logger.LogInformation($"GetFileAttributes: {Request.QueryString}");

      return new
      {
        success = false,
        entryName = path,
        attrHidden = false
      };
    }

    [Route("tcc/Del")]
    [HttpGet]
    public dynamic Del([FromQuery] string ticket, [FromQuery]string filespaceid, [FromQuery]string path, [FromQuery]bool recursive)
    {
      Logger.LogInformation($"Del: {Request.QueryString}");

      return new
      {
        success = true,
        path = path
      };
    }

    [Route("tcc/MkDir")]
    [HttpGet]
    public dynamic MkDir([FromQuery] string ticket, [FromQuery]string filespaceid, [FromQuery]string path, [FromQuery]bool force)
    {
      Logger.LogInformation($"MkDir: {Request.QueryString}");

      return new
      {
        success = true,
        path = path,
        entryId = Guid.NewGuid()
      };
    }

    [Route("tcc/Dir")]
    [HttpGet]
    public dynamic Dir([FromQuery] string ticket, [FromQuery]string filespaceid, [FromQuery]string path, [FromQuery]bool recursive, [FromQuery]bool filterfolders, [FromQuery]string filemasks)
    {
      Logger.LogInformation($"Dir: {Request.QueryString}");

      if (filespaceid == "u710e3466-1d47-45e3-87b8-81d1127ed4ed")//Mock TBC customer filespace
      {
        return new
        {
          success = true,
          createTime = DateTime.UtcNow,
          entries = new[]
          {
            new
            {
              createTime = DateTime.UtcNow,
              entryName = filemasks,
              isFolder = false,
              leaf = true,
              modifyTime = DateTime.UtcNow,
              size = 1182
            }
          },
          isFolder = true,
          leaf = false,
          modifyTime = DateTime.UtcNow,
          size = 16348
        };
      }

      return new
      {
        success = true,
        createTime = DateTime.UtcNow,
        entryName = filemasks,
        entries = new[] { new { } },
        isFolder = false,
        leaf = true,
        modifyTime = DateTime.UtcNow,
        size = 7284
      };
    }

    [Route("tcc/Login")]
    [HttpGet]
    public dynamic Login([FromQuery]string username, [FromQuery]string orgname, [FromQuery]string password, [FromQuery]string mode, [FromQuery]bool forcegmt)
    {
      Logger.LogInformation($"Login: {Request.QueryString}");

      return new
      {
        success = true,
        ticket = "TICKET_a057b8760674a1ab8187936e850e85515f74dfca"
      };
    }

    [Route("tcc/CreateFileJob")]
    [Route("tcc/CheckFileJobStatus")]
    [Route("tcc/GetFileJobResult")]
    [Route("tcc/ExportToWebFormat")]
    [Route("tcc/CheckExportJob")]
    [Route("tcc/Ren")]
    [Route("tcc/LastDirChange")]
    [HttpGet]
    public dynamic NotImplementedMethods([FromQuery] string ticket)
    {
      throw new NotImplementedException("Not implemented in mock web api");
    }
  }
}
