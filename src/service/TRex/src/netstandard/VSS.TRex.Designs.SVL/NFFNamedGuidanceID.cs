﻿using VSS.TRex.Common;

namespace VSS.TRex.Designs.SVL
{
  public class NFFNamedGuidanceID
  {
    public string Name;
    public string Description;

    private int _id;

    public int ID
    {
      get => _id;
      set => SetID(value);
    }

    private byte _flags;

    public byte Flags
    {
      get => _flags;
      set => SetFlags(value);
    }

    public double StartStation;
    public double EndStation;
    public double StartOffset;

    private NFFGuidableAlignmentEntity _guidanceAlignment;

    public NFFGuidableAlignmentEntity GuidanceAlignment
    {
      get => _guidanceAlignment;
      set => SetGuidanceAlignment(value);
    }

    // In general collections of NamedGuidanceIDs are sorted by Offset, however
    // TE can generate some "special" sub-alignments (hinge, ditch and batter),
    // identified by tags in their Name strings, that need to appear in a fixed
    // order in the sorted listed.  This field keeps track of whether the the
    // alignment is a "special" one for the benefit of the
    // NFFNamedGuidanceIDList.SortByOffset method but is not otherwise used
    // (and is not streamed to/from file)
    public NFFGuidanceAlignmentType GuidanceAlignmentType;

    // A temporary reference to the object that NFFNamedGuidanceID is being
    // created from
    // FTag : TObject;

    private void SetID(int Value)
    {
      _id = Value;

      if (_guidanceAlignment != null)
        _guidanceAlignment.GuidanceID = Value;
    }

    private void SetGuidanceAlignment(NFFGuidableAlignmentEntity Value)
    {
      _guidanceAlignment = Value;

      if (_guidanceAlignment != null)
        _guidanceAlignment.GuidanceID = ID;
    }

    private void SetFlags(byte Value)
    {
      _flags = Value;

      // Ensure fGuidanceAlignmentType is consistent with flags
      if ((_flags & NFFConsts.kNFFGuidanceIDHasStationing) != 0)
        GuidanceAlignmentType = NFFGuidanceAlignmentType.gtMasterAlignment;
      else if (GuidanceAlignmentType == NFFGuidanceAlignmentType.gtMasterAlignment)
        GuidanceAlignmentType = NFFGuidanceAlignmentType.gtSubAlignment;
    }

    public NFFNamedGuidanceID()
    {
      Name = string.Empty;
      Description = string.Empty;
      ID = -1;
      Flags = 0;
      StartStation = Consts.NullDouble;
      EndStation = Consts.NullDouble;
      StartOffset = Consts.NullDouble;
      GuidanceAlignment = null;
      GuidanceAlignmentType = NFFGuidanceAlignmentType.gtSubAlignment;
      //Tag= Nil;
    }

    public NFFNamedGuidanceID(string AName) : this()
    {
      Name = AName;
    }

    //  procedure Assign(NamedGuidanceID: NFFNamedGuidanceID);

    //      property Tag : TObject read FTag write FTag;

    //      function Clone: NFFNamedGuidanceID;
//      procedure DumpToText(Stream: TTextDumpStream);
  }
}
