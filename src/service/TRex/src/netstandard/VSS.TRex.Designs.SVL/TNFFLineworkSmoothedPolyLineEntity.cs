﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using VSS.TRex.Common;
using VSS.TRex.Common.Utilities;
using VSS.TRex.Geometry;

namespace VSS.TRex.Designs.SVL
{

  public class TNFFLineworkSmoothedPolyLineEntity : TNFFStationedLineworkEntity
  {

    public List<TNFFLineworkSmoothedPolyLineVertexEntity> Vertices;// TNFFLineworkSmoothedPolyLineVertexEntityList

    //   fPolylineProcessed: Boolean;
    //   fCreatedFromStream: Boolean;

    public TNFFLineworkSmoothedPolyLineEntity()
    {
      ElementType = TNFFLineWorkElementType.kNFFLineWorkSmoothedPolyLineElement;
      Vertices=  new List<TNFFLineworkSmoothedPolyLineVertexEntity>();

    //  fCreatedFromStream= False;
    //  fPolylineProcessed= False;
    }

    protected override double GetStartStation()
    {
      double result = Consts.NullDouble;

      if (Vertices.Count > 0)
      {
        result = Vertices.First().Chainage;
      }

      return result;
    }

    protected override void SetStartStation(double Value)
    {
      // Illegal to set StartStation as it is determined by the chainage of the first Vertex
      // Ignore it...
    }

    protected override double GetEndStation()
    {
      double result = Consts.NullDouble;
      if (Vertices.Count > 0)
        result = Vertices.Last().Chainage;
      return result;
    }

    protected override double GetVertexElevation(int VertexNum)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1),
        "VertexNum out of range in TNFFLineworkSmoothedPolyLineEntity.GetVertexElevation");

      return Vertices[VertexNum].Z;
    }

    protected override void SetVertexElevation(int VertexNum, double Value)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1),
        "VertexNum out of range in TNFFLineworkSmoothedPolyLineEntity.SetVertexElevation");

      Vertices[VertexNum].Z = Value;
    }

    protected override double GetVertexStation(int VertexNum)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1),
        "VertexNum out of range in TNFFLineworkSmoothedPolyLineEntity.GetVertexStation");

      return Vertices[VertexNum].Chainage;
    }

    protected override void SetVertexStation(int VertexNum, double Value)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1),
        "VertexNum out of range in TNFFLineworkSmoothedPolyLineEntity.SetVertexStation");

      Vertices[VertexNum].Chainage = Value;
    }

    /*
    function GetVertexLeftCrossSlope(VertexNum: Integer): Double; Override;
    procedure SetVertexLeftCrossSlope(VertexNum: Integer;
    const Value: Double); Override;

    function GetVertexRightCrossSlope(VertexNum: Integer): Double; Override;
    procedure SetVertexRightCrossSlope(VertexNum: Integer;
    const Value: Double); Override;
    */

    //public  procedure Assign(Entity: TNFFLineworkEntity); override;

    // procedure DumpToText(Stream: TTextDumpStream; const OriginX, OriginY : Double); override;

    // The SavetoSream/LoadFromNFFStream method with origin parameters are intended
    // for saving this entity to an NFF file.
    //  Procedure SaveToNFFStream(Stream : TStream;
    //  const OriginX, OriginY : Double;
    //                            FileVersion : TNFFFileVersion); Overload; Override;
    public override void LoadFromNFFStream(BinaryReader reader,
      double OriginX, double OriginY,
      bool HasGuidanceID,
      TNFFFileVersion FileVersion)
    {
      // I : integer;
      //     X, Y, Z, Chainage: Double;
      //   VertexCount: Integer;
      //     FLeftCrossSlope, FRightCrossSlope: Double;

      //  fCreatedFromStream = True;
      //fPolylineProcessed= True;

      double Alpha = Consts.NullDouble; // Keep compiler quite
      double Beta = Consts.NullDouble; // Keep compiler quite

      try
      {
        // Prevent <fVertices> list complaining about additions to processed polyline
        //fSuppressAssertions= True;

        // There is no need to read the entity type as this will have already been
        // read in order to determine we should be reading this type of entity!

        if (HasGuidanceID)
          GuidanceID = reader.ReadUInt16();

        // Read in the linewidth (always 1)
        var _ = reader.ReadByte();

        // Read out the rest of the information for the entity
        Colour = NFFUtils.ReadColourFromStream(reader);

        // Read the flags...
        EntityFlags = reader.ReadByte();

        // Read the bounding box of the smoothed polyline
        NFFUtils.ReadRectFromStream(reader, out double MinX, out double MinY, out double MaxX, out double MaxY, OriginX, OriginY);

        // Read the number of vertices in the polyline (this includes the first point)
        int VertexCount = reader.ReadInt16();

        // Read in the list vertices from the stream
        for (int I = 1; I < VertexCount + 1; I++)
        {
          NFFUtils.ReadCoordFromStream(reader, out double X, out double Y, OriginX, OriginY);

          double Z;
          if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasElevation) != 0)
            Z = NFFUtils.ReadFixedPoint32FromStream(reader);
          else
            Z = Consts.NullDouble;

          double LeftCrossSlope;
          double RightCrossSlope;
          if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasCrossSlope) != 0)
            NFFUtils.ReadCrossSlopeInformationFromStream(reader, out LeftCrossSlope, out RightCrossSlope);
          else
          {
            LeftCrossSlope = Consts.NullDouble;
            RightCrossSlope = Consts.NullDouble;
          }

          if (I > 1)
          {
            Alpha = reader.ReadSingle();
            Beta = reader.ReadSingle();
          }

          double Chainage;
          if ((HeaderFlags & NFFConsts.kNFFElementHeaderHasStationing) != 0)
            Chainage = reader.ReadDouble();
          else
            Chainage = Consts.NullDouble;

          Vertices.Add(new TNFFLineworkSmoothedPolyLineVertexEntity(this, X, Y, Z, Chainage, Consts.NullDouble));
          if (I > 1)
          {
            Vertices.Last().Alpha = Alpha;
            Vertices.Last().Beta = Beta;
          }

          Vertices.Last().LeftCrossSlope = LeftCrossSlope;
          Vertices.Last().RightCrossSlope = RightCrossSlope;
        }
      }
      finally
      {
        // fSuppressAssertions= False;
      }
    }

    // These SaveToStream/LoadFromStream methods implement GENERIC save/load functionality
    // NOT NFF save/load functionality
    //   Procedure SaveToStream(Stream : TStream); Overload; override;
    //   Procedure LoadFromStream(Stream : TStream); Overload; override;



    public override BoundingWorldExtent3D BoundingBox()
    {
      if (Vertices.Count == 0)
        return new BoundingWorldExtent3D(Consts.NullDouble, Consts.NullDouble, Consts.NullDouble, Consts.NullDouble);

      if (Vertices.Count == 1)
        return new BoundingWorldExtent3D(Vertices.First().X, Vertices.First().Y, Vertices.First().X, Vertices.First().Y);

      var Result = new BoundingWorldExtent3D(Math.Min(Vertices[0].X, Vertices[1].X),
        Math.Min(Vertices[0].Y, Vertices[1].Y),
        Math.Max(Vertices[0].X, Vertices[1].X),
        Math.Max(Vertices[0].Y, Vertices[1].Y));

      for (int I = 2; I < Vertices.Count; I++)
        Result.Include(Vertices[I].X, Vertices[I].Y);

      return Result;
    }


    public override bool HasValidHeight()
    {
      if (ControlFlag_NullHeightAllowed)
        return false;

      for (int I = 0; I < Vertices.Count; I++)
        if (!Vertices[I].HasValidHeight())
          return false;

      return true;
    }

    public override XYZ GetStartPoint()
    {
      if (Vertices.Count == 0)
        return base.GetStartPoint();

      return Vertices.First().AsXYZ();
    }

    public override XYZ GetEndPoint()
    {
      if (Vertices.Count == 0)
        return base.GetStartPoint();

      return Vertices.Last().AsXYZ();
    }

    //   procedure ProcessPolyLine;

    //   function Concatenate(SmoothedPolyline: TNFFLineworkSmoothedPolyLineEntity): Boolean;

    //   Procedure Reverse; Override;

    public override void ComputeStnOfs(double X, double Y, out double Stn, out double Ofs)
    {
      Stn = Consts.NullDouble;
      Ofs = Consts.NullDouble;

      int ClosestVertex = -1;
      double ClosestDistance = 1E99;
      double ClosestT = Consts.NullDouble;

      //  writeln(LogFile, '[SP] Start Calc'); {SKIP}

      // Locate the interval in the smooth polyline the station value refers to.
      for (int I = 0; I < Vertices.Count - 1; I++)
      {
        NFFUtils.DistanceToNFFCurve(Vertices[I].X, Vertices[I].Y,
          Vertices[I + 1].X, Vertices[I + 1].Y,
          X, Y,
          Vertices[I + 1].Alpha,
          Vertices[I + 1].Beta,
          out Ofs, out double t);

        if (Range.InRange(t, 0 - 0.001, 1 + 0.001))
        {
          if (Math.Abs(Ofs) < Math.Abs(ClosestDistance))
          {
            ClosestDistance = Ofs;
            ClosestVertex = I;
            ClosestT = t;
          }
        }
      }

      if (ClosestVertex > -1)
      {
        Stn = Vertices[ClosestVertex].Chainage + (ClosestT * (Vertices[ClosestVertex + 1].Chainage - Vertices[ClosestVertex].Chainage));
        Ofs = ClosestDistance;
      }
      else
        Ofs = Consts.NullDouble;
    }

    public override void ComputeXY(double Stn, double Ofs, out double X, out double Y)
    {
   //   var
 //     I : integer;
   //   Index: Integer;
 //     t: Extended;
 //     SegmentChainageLength: Extended;
 //     VertexAtIndex: TNFFLineworkSmoothedPolyLineVertexEntity;
//      VertexAtIndexPlus1: TNFFLineworkSmoothedPolyLineVertexEntity;
 //     begin

      int Index = -1;
      X= Consts.NullDouble;
      Y= Consts.NullDouble;

      // Locate the interval in the smooth polyline the station value refers to.
      for (int I = 0; I < Vertices.Count - 1; I++)
        if ((Vertices[I].Chainage <= (Stn + 0.0001)) && (Vertices[I + 1].Chainage > (Stn - 0.0001)))
        {
          Index = I;
          break;
        }

      if (Index == -1)
        return;

      double SegmentChainageLength= (Vertices[Index + 1].Chainage - Vertices[Index].Chainage);
      double t = (Stn - Vertices[Index].Chainage) / SegmentChainageLength;

      //  writeln(LogFile, '[SP] Start Calc'); {SKIP}

      var VertexAtIndex = Vertices[Index];
      var VertexAtIndexPlus1 = Vertices[Index + 1];

      NFFUtils.CalcNFFCurvePosFromParmAndOfs(VertexAtIndex.X, VertexAtIndex.Y,
        VertexAtIndexPlus1.X, VertexAtIndexPlus1.Y,
        VertexAtIndexPlus1.Alpha, VertexAtIndexPlus1.Beta,
        t, Ofs,
        out X, out Y);

      //  writeln(LogFile, Format('[SP] Calcing XY from %.4f/%.4f, Index=%d, t=%.6f, scl=%.4f [Result: X=%.4f, Y=%.4f]', {SKIP}
      //                          [stn, ofs, Index, T, SegmentChainageLength, X, Y]));
      //  writeln(LogFile, Format('[SP] Source parms = %.4f, %.4f, %.4f, %.4f, %.4f, %.4f, %.4f, %.4f', {SKIP}
      //                          [FVertices[Index].X, FVertices[Index].Y,
      //                           FVertices[Index + 1].X, FVertices[Index + 1].Y,
      //                           FVertices[Index + 1].Alpha, FVertices[Index + 1].Beta,
      //                           t, Ofs]));

      //  if Length < (Distance - 0.0001) then
      //    begin
      //      Assert(False, 'Length < Distance in TNFFLineworkSmoothedPolyLineEntity.ComputeXY'); {SKIP}
      //      Exit;
      //    end;
    }

    //    Procedure UpdateHeight(const UpdateIfNullOnly : Boolean;
    //                           const Position : TXYZ;
    //                          const Station : Double;
    //                        const Index : Integer); Override;

    //  procedure ResetStartStation(const NewStartStation : Double); Override;

    public override double ElementLength()
    {
      double Result = 0;

      if (Vertices.Count > 0)
      Result = EndStation - StartStation;

      return Result;
    }

    public override double ElementLength(int Index)
    {
      if (!Range.InRange(Index, 0, Vertices.Count - 1))
      {
        Debug.Assert(false, "Out of range vertex index in TNFFLineworkSmoothedPolyLineEntityElementLength");
        return 0;
      }

      if (Index == Vertices.Count - 1)
      {
        return 0;
      }
      return Vertices[Index + 1].Chainage - Vertices[Index].Chainage;
    }

 //   procedure SetDefaultStationing(const AStartStation : Double;
 //                                  AIndex : Integer); Override;

 public override bool HasInternalStructure() => true;

//    Function IsSameAs(const Other : TNFFLineworkEntity) : Boolean; Override;

    public override int VertexCount() => Vertices.Count;

    public override TNFFLineworkPolyLineVertexEntity GetVertex(int VertexNum)
    {
      Debug.Assert(Range.InRange(VertexNum, 0, Vertices.Count - 1), "Vertex index out of range");

      return Vertices[VertexNum];
    }

    //   Procedure InsertVertex(Vertex : TNFFLineworkPolyLineVertexEntity;
    //   InsertAt : Integer); Override;

    // CreateVertexAtStation creates a new vertex at the requested station. The station value must
    // lie between the station values of two surrounding vertices. The other values for the vertex are
    // calculated from those of the surrounding vertices.
    //   Function CreateVertexAtStation(const Chainage : Double) : TNFFLineworkPolyLineVertexEntity; Override;

    //   Function CreateNewVertex : TNFFLineworkPolyLineVertexEntity; Override;

    // ComputeGeometricStationing calculates the chainage of each vertex as as geometric
    // distance along the curve from the start of the element
   // public override void ComputeGeometricStationing()
  //   Function InMemorySize : Longint; Override;
  }
}