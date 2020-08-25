﻿using Apache.Ignite.Core.Binary;
using VSS.TRex.Common;
using VSS.TRex.SubGridTrees;
using VSS.TRex.SubGridTrees.Interfaces;

namespace VSS.TRex.Designs.GridFabric.Responses
{
  public class DesignFilterSubGridMaskResponse : BaseDesignRequestResponse
  {
    private const byte VERSION_NUMBER = 1;

    public SubGridTreeBitmapSubGridBits Bits;

    public override void ToBinary(IBinaryRawWriter writer)
    {
      base.ToBinary(writer);

      VersionSerializationHelper.EmitVersionByte(writer, VERSION_NUMBER);

      writer.WriteBoolean(Bits != null);

      if (Bits != null)
      {
        int[] buffer = new int[SubGridTreeConsts.SubGridTreeDimension];
        for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          buffer[i] = unchecked((int) Bits.Bits[i]);

        writer.WriteIntArray(buffer);
      }
    }

    public override void FromBinary(IBinaryRawReader reader)
    {
      base.FromBinary(reader);

      VersionSerializationHelper.CheckVersionByte(reader, VERSION_NUMBER);

      if (reader.ReadBoolean())
      {
        Bits = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Unfilled);

        int[] buffer = reader.ReadIntArray();
        for (int i = 0; i < SubGridTreeConsts.SubGridTreeDimension; i++)
          Bits.Bits[i] = unchecked((uint) buffer[i]);
      }
    }
  }
}
