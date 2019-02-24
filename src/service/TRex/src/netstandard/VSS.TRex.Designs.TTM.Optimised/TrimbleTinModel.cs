﻿using System;
using System.IO;
using VSS.TRex.Designs.TTM.Optimised.Exceptions;

namespace VSS.TRex.Designs.TTM.Optimised
{
  public class TrimbleTINModel
  {
    /// <summary>
    /// The full set of vertices that make up this TIN model
    /// </summary>
    public TriVertices Vertices { get; set; } = new TriVertices();

    /// <summary>
    /// The full set of triangles that make up this TIN model
    /// </summary>
    public Triangles Triangles { get; set; } = new Triangles();

    /// <summary>
    /// The set of triangles that comprise the edge of the TIN
    /// </summary>
    public TTMEdges Edges { get; } = new TTMEdges();

    /// <summary>
    /// The set of start points defined for the TIN
    /// </summary>
    public TTMStartPoints StartPoints { get; } = new TTMStartPoints();

    /// <summary>
    /// The header information stored with a TIN surface in a TTM file
    /// </summary>
    public TTMHeader Header = TTMHeader.NewHeader();

    public string ModelName { get; set; }

    public TrimbleTINModel()
    {
    }

    /// <summary>
    /// Reads a TrimbleTINModel using the provided reader
    /// </summary>
    /// <param name="reader"></param>
    /// <param name="bytes"></param>
    public void Read(BinaryReader reader, byte[] bytes)
    {
      string LoadErrMsg = "";

      try
      {
        LoadErrMsg = "Error reading header";

        Header.Read(reader);

        var identifier = System.Text.Encoding.ASCII.GetString(Header.FileSignature);
        if (identifier != Consts.TTM_FILE_IDENTIFIER)
        {
          throw new TTMFileReadException("File is not a Trimble TIN Model.");
        }

        // Check file version
        if (Header.FileMajorVersion != Consts.TTM_MAJOR_VERSION
            || Header.FileMinorVersion != Consts.TTM_MINOR_VERSION)
        {
          throw new TTMFileReadException($"TTM_Optimized.Read(): Unable to read this version {Header.FileMajorVersion}: {Header.FileMinorVersion} of Trimble TIN Model file. Expected version: { Consts.TTM_MAJOR_VERSION}: {Consts.TTM_MINOR_VERSION}");
        }

        // ModelName = (String)(InternalNameToANSIString(Header.DTMModelInternalName));
        // Not handled for now
        ModelName = "Reading not implemented";

        LoadErrMsg = "Error reading vertices";
        reader.BaseStream.Position = Header.StartOffsetOfVertices;
        Vertices.Read(reader, Header);
        //Vertices.Read(bytes, Header.StartOffsetOfVertices, Header);

        LoadErrMsg = "Error reading triangles";
        //reader.BaseStream.Position = Header.StartOffsetOfTriangles;
        //Triangles.Read(reader, Header);
        Triangles.Read(bytes, Header.StartOffsetOfTriangles, Header);

        LoadErrMsg = "Error reading edges";
        reader.BaseStream.Position = Header.StartOffsetOfEdgeList;
        Edges.Read(reader, Header);

        LoadErrMsg = "Error reading start points";
        reader.BaseStream.Position = Header.StartOffsetOfStartPoints;
        StartPoints.Read(reader, Header);
      }
      catch (TTMFileReadException)
      {
        throw; // pass it on
      }
      catch (Exception E)
      {
        throw new TTMFileReadException($"Exception at TTM loading phase {LoadErrMsg}", E);
      }
    }

    /// <summary>
    /// Loads a TrimbleTINModel from a stream
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="bytes"></param>
    public void LoadFromStream(Stream stream, byte [] bytes)
    {
      using (BinaryReader reader = new BinaryReader(stream))
      {
        Read(reader, bytes);
      }
    }

    /// <summary>
    /// Loads a TrimbleTINModel from a stream
    /// </summary>
    /// <param name="FileName"></param>
    public void LoadFromFile(string FileName)
    {
      byte[] bytes = File.ReadAllBytes(FileName);

      using (MemoryStream ms = new MemoryStream(bytes))
      {
        LoadFromStream(ms, bytes);
      }

      // FYI, This method sucks totally - don't use it
      //using (FileStream fs = new FileStream(FileName, FileMode.Open, FileAccess.Read, FileShare.Read, 2048))
      //{
      //    LoadFromStream(fs);
      //}

      if (ModelName.Length == 0)
      {
        ModelName = Path.ChangeExtension(Path.GetFileName(FileName), "");
      }
    }
  }
}
