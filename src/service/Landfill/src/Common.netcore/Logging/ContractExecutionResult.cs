﻿namespace LandfillService.Common.Contracts
{
  public enum ContractExecutionStatesEnum
  {
    ExecutedSuccessfully,
    InternalProcessingError,
    IncorrectRequestedData
  }

  /// <summary>
  ///   Represents general (minimal) reponse generated by a sevice. All other responses should be derived from this class.
  /// </summary>
  public class ContractExecutionResult
  {
    /// <summary>
    ///   Initializes a new instance of the <see cref="ContractExecutionResult" /> class.
    /// </summary>
    /// <param name="code">
    ///   The resulting code. Default value is <see cref="ContractExecutionStates.ExecutedSuccessfully" />
    /// </param>
    /// <param name="message">The verbose user-friendly message. Default value is empty string.</param>
    public ContractExecutionResult(ContractExecutionStatesEnum code, string message = "")
    {
      Code = code;
      Message = message;
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="ContractExecutionResult" /> class with default
    ///   <see cref="ContractExecutionStates.ExecutedSuccessfully" /> result
    /// </summary>
    /// <param name="message">The verbose user-friendly message.</param>
    protected ContractExecutionResult(string message) : this(ContractExecutionStatesEnum.ExecutedSuccessfully, message)
    {
    }

    /// <summary>
    ///   Initializes a new instance of the <see cref="ContractExecutionResult" /> class with default
    ///   <see cref="ContractExecutionStates.ExecutedSuccessfully" /> result and "success" message
    /// </summary>
    public ContractExecutionResult()
      : this("success")
    {
    }


    /// <summary>
    ///   Defines machine-readable code.
    /// </summary>
    /// <value>
    ///   Result code.
    /// </value>
    public ContractExecutionStatesEnum Code { get; protected set; }

    /// <summary>
    ///   Defines user-friendly message.
    /// </summary>
    /// <value>
    ///   The message string.
    /// </value>
    public string Message { get; protected set; }

    /// <summary>
    ///   Gets the help sample.
    /// </summary>
    /// <value>
    ///   The help sample.
    /// </value>
    public static ContractExecutionResult HelpSample => new ContractExecutionResult("success");
  }
}