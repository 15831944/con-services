﻿using System;

namespace VSS.Tpaas.Client.Models
{
  public sealed class TPaaSClientState : ITPaaSClientState
  {
    public string TPaaSToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = string.Empty;
    public DateTime TPaaSTokenExpiry { get; set; } = DateTime.MinValue;

    private static readonly Lazy<TPaaSClientState> _instance = new Lazy<TPaaSClientState>(() => new TPaaSClientState());

    public static TPaaSClientState Instance => _instance.Value;

    private TPaaSClientState()
    { }
  }
}
