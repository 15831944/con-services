﻿using System;
using VSS.VisionLink.Interfaces.Events.MasterData.Models;

namespace MasterDataProxies.Models
{
  /// <summary>
  /// Describes an imported file for a project
  /// </summary>
  public class FileData
  {
    /// <summary>
    /// Gets or sets the Project uid.
    /// </summary>
    /// <value>
    /// The Project uid.
    /// </value>
    public string ProjectUid { get; set; }

    /// <summary>
    /// Gets or sets the file uid.
    /// </summary>
    /// <value>
    /// The file uid.
    /// </value>
    public string ImportedFileUid { get; set; }

    /// <summary>
    /// Gets or sets the customer uid.
    /// </summary>
    /// <value>
    /// The customer uid.
    /// </value>
    public string CustomerUid { get; set; }

    /// <summary>
    /// Gets or sets the type of the file.
    /// </summary>
    /// <value>
    /// The type of the file.
    /// </value>
    public ImportedFileType ImportedFileType { get; set; }

    /// <summary>
    /// Gets the name of the file type.
    /// </summary>
    /// <value>
    /// The name of the file type.
    /// </value>
    public string ImportedFileTypeName => this.ImportedFileType.ToString();

    /// <summary>
    /// Gets or sets the name of the file.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the timestamp at which the file was created on users file system.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public DateTime FileCreatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the timestamp at which the file was updated on users file system.
    /// </summary>
    /// <value>
    /// The name.
    /// </value>
    public DateTime FileUpdatedUtc { get; set; }

    /// <summary>
    /// Gets or sets the ImportedBy email address from authentication
    /// </summary>
    /// <value>
    /// The users email address.
    /// </value>
    public string ImportedBy { get; set; }

    /// <summary>
    /// Gets or sets the Surveyed Utc. 
    /// This only applies to a file type of SurveyedSurface, else null.
    /// </summary>
    /// <value>
    /// The start date.
    /// </value>
    public DateTime? SurveyedUtc { get; set; }

    /// <summary>
    /// The Utc when the file was last imported
    /// </summary>
    /// <value>
    /// Utc date
    /// </value>
    public DateTime ImportedUtc { get; set; }

    /// <summary>
    /// Gets or sets the Activation State of the imported file.
    /// </summary>
    public bool IsActivated { get; set; }
    /// <summary>
    /// Gets the path of the imported file
    /// </summary>
    /// <value>
    /// Path of the imported file
    /// </value>
    public string Path => "/" + CustomerUid + "/" + ProjectUid;
  }
}
