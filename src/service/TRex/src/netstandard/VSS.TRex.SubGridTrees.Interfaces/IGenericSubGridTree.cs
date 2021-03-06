﻿using System;

namespace VSS.TRex.SubGridTrees.Interfaces
{
  /// <summary>
  /// 
  /// </summary>
  /// <typeparam name="T"></typeparam>
  /// <typeparam name="S"></typeparam>
  public interface IGenericSubGridTree<T, S> : ISubGridTree where S : IGenericLeafSubGrid<T>, ILeafSubGrid
  {
    T this[int x, int y] { get; set; }

    void ForEach(Func<T, bool> functor);
  }
}
