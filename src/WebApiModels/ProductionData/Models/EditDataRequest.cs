﻿using System.ComponentModel.DataAnnotations;
using System.Net;
using Newtonsoft.Json;
using VSS.Productivity3D.Common.Contracts;
using VSS.Productivity3D.Common.Interfaces;
using VSS.Productivity3D.Common.Models;
using VSS.Productivity3D.Common.ResultHandling;

namespace VSS.Productivity3D.WebApiModels.ProductionData.Models
{
  /// <summary>
  /// A representation of an edit request. This request gives a user the ability to correct data that has been recorded wrongly in Machines by Operators.
  /// A previously applied edit can also be undone.
  /// </summary>
  public class EditDataRequest : ProjectID, IValidatable
  {

    /// <summary>
    /// Flag which determines if the edit is applied or undone. Required.
    /// </summary>
    [JsonProperty(PropertyName = "undo", Required = Required.Always)]
    [Required]
    public bool undo { get; private set; }

    /// <summary>
    /// Details of the edit to apply or undo. Required for applying an edit and for a single undo.
    /// If null and undo is true then all edits to the production data for the project will be undone.
    /// </summary>
    [JsonProperty(PropertyName = "dataEdit", Required = Required.Default)]
    public ProductionDataEdit dataEdit { get; private set; }
  

      /// <summary>
    /// Private constructor
    /// </summary>
    private EditDataRequest()
    {
    }


    /// <summary>
    /// Create instance of EditDataRequest
    /// </summary>
    public static EditDataRequest CreateEditDataRequest(
      long projectId,
      bool undo,
      ProductionDataEdit dataEdit
      )
    {
      return new EditDataRequest
             {
                 projectId = projectId,
                 undo = undo,
                 dataEdit = dataEdit
             };
    }


    /// <summary>
    /// Create example instance of EditDataRequest to display in Help documentation.
    /// </summary>
    public new static EditDataRequest HelpSample
    {
      get
      {
        return new EditDataRequest
        {
                   projectId = 1523,
                   undo = false,
                   dataEdit = ProductionDataEdit.HelpSample
               };
      }
    }

    /// <summary>
    /// Validates all properties
    /// </summary>
    public override void Validate()
    {
      base.Validate();
      if (!undo && dataEdit == null)
        throw new ServiceException(HttpStatusCode.BadRequest,
                new ContractExecutionResult(ContractExecutionStatesEnum.ValidationError,
                    "Missing data edit for edit request"));

      if (dataEdit != null)
        dataEdit.Validate();
    }

  }
}