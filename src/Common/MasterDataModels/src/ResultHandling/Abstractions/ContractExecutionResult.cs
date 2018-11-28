﻿using Newtonsoft.Json;

namespace VSS.MasterData.Models.ResultHandling.Abstractions
{
  /// <summary>
  ///   Represents general (minimal) reponse generated by a sevice. All other responses should be derived from this class.
  /// </summary>
  public class ContractExecutionResult
  {
    public const string DefaultMessage = "success";

    /// <summary>
    ///   Defines machine-readable code.
    /// </summary>
    /// <value>
    ///   Result code.
    /// </value>
    [JsonProperty(PropertyName = "Code", Required = Required.Always)]
    public int Code { get; protected set; }

    /// <summary>
    ///   Defines user-friendly message.
    /// </summary>
    /// <value>
    ///   The message string.
    /// </value>
    [JsonProperty(PropertyName = "Message", Required = Required.Always)]
    public string Message { get; protected set; }

    /// <summary>
    ///   Initializes a new instance of the <see cref="ContractExecutionResult" /> class with default
    ///   <see cref="ContractExecutionStatesEnum.Success" /> result and "success" message
    /// </summary>
    public ContractExecutionResult()
      : this(DefaultMessage)
    { }

    /// <summary>
    ///   Initializes a new instance of the <see cref="ContractExecutionResult" /> class.
    /// </summary>
    /// <param name="code">
    ///   The resulting code. Default value is <see cref="ContractExecutionStatesEnum.Success" />
    /// </param>
    /// <param name="message">The verbose user-friendly message. Default value is empty string.</param>
    public ContractExecutionResult(int code, string message = DefaultMessage)
    {
      Code = code;
      Message = message;
    }

    public static ContractExecutionResult ErrorResult(string errorMessage = "Unhandled error state") => new ContractExecutionResult(
      ContractExecutionStatesEnum.InternalProcessingError,
      errorMessage);

    /// <summary>
    ///   Initializes a new instance of the <see cref="ContractExecutionResult" /> class with default
    ///   <see cref="ContractExecutionStatesEnum.Success" /> result
    /// </summary>
    /// <param name="message">The verbose user-friendly message.</param>
    protected ContractExecutionResult(string message)
      : this(ContractExecutionStatesEnum.ExecutedSuccessfully, message)
    { }
  }
}
