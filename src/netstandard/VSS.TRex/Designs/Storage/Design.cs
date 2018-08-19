﻿using System;
using System.IO;
using VSS.TRex.Designs.GridFabric.Arguments;
using VSS.TRex.Designs.GridFabric.Requests;
using VSS.TRex.Geometry;
using VSS.TRex.SubGridTrees.Interfaces;
using VSS.TRex.Utilities.Interfaces;
using VSS.TRex.Utilities.ExtensionMethods;
using VSS.TRex.Designs.Interfaces;
using VSS.TRex.Designs.Models;
using VSS.TRex.SubGridTrees.Client.Interfaces;

namespace VSS.TRex.Designs.Storage
{
  /// <summary>
    /// Represents the information known about a design
    /// </summary>
    [Serializable]
    public class Design : IEquatable<IDesign>, IBinaryReaderWriter, IDesign
  {
        /// <summary>
        /// Singleton request used by all designs. This request encapsulates the Ignite reference which
        /// is relatively slow to initialise when making many calls.
        /// </summary>
        private static DesignElevationPatchRequest request = new DesignElevationPatchRequest();

        /// <summary>
        /// Binary serialization logic
        /// </summary>
        /// <param name="writer"></param>
        public void Write(BinaryWriter writer)
        {
            writer.Write(ID.ToByteArray());
            DesignDescriptor.Write(writer);
            Extents.Write(writer);
        }

        /// <summary>
        /// Binary serialization logic
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="buffer"></param>
        public void Write(BinaryWriter writer, byte[] buffer) => Write(writer);

        /// <summary>
        /// Binary deserialization logic
        /// </summary>
        /// <param name="reader"></param>
        public void Read(BinaryReader reader)
        {
            ID = reader.ReadGuid();
            DesignDescriptor.Read(reader);
            Extents.Read(reader);
        }

        /// <summary>
        /// The intenal identifier of the design
        /// </summary>
        public Guid ID { get; private set; } = Guid.Empty;

      /// <summary>
      /// The full design descriptior representing the design
      /// </summary>
      public DesignDescriptor DesignDescriptor;

    // Public accessor method for design descriptor struct
    public DesignDescriptor Get_DesignDescriptor() => DesignDescriptor;

      /// <summary>
    /// The rectangular bounding extents of the design in grid coordiantes
    /// </summary>
    private BoundingWorldExtent3D extents;

        /// <summary>
        /// No-arg constructor
        /// </summary>
        public Design()
        {
        }

      /// <summary>
      /// Returns the real world 3D enclosing extents for the surveyed surface topology, including any configured vertical offset
      /// </summary>
      public BoundingWorldExtent3D Extents
      {
        get
        {
          BoundingWorldExtent3D result = new BoundingWorldExtent3D(extents);

          // Incorporate any vertical offset from the underlying design the surveyed surface is based on
          result.Offset(DesignDescriptor.Offset);

          return result;
        }
      }

        /// <summary>
    /// Constructor accepting a Binary Reader instance from which to instantiate itself
    /// </summary>
    /// <param name="reader"></param>
    public Design(BinaryReader reader)
        {
            Read(reader);
        }

        /// <summary>
        /// Constructor accepting full design state
        /// </summary>
        /// <param name="iD"></param>
        /// <param name="designDescriptor"></param>
        /// <param name="extents"></param>
        public Design(Guid iD,
                      DesignDescriptor designDescriptor,
                      BoundingWorldExtent3D extents_)
        {
            ID = iD;
            DesignDescriptor = designDescriptor;
            extents = extents_;
        }

        /// <summary>
        /// Produces a deep clone of the design
        /// </summary>
        /// <returns></returns>
        public IDesign Clone() => new Design(ID, DesignDescriptor, new BoundingWorldExtent3D(Extents));

        /// <summary>
        /// ToString() for Design
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"ID:{ID}, DesignID:{DesignDescriptor.DesignID}; {DesignDescriptor.FileSpace};{DesignDescriptor.Folder};{DesignDescriptor.FileName} {DesignDescriptor.Offset:F3} [{Extents}]";
        }

        /// <summary>
        /// Determine if two designs are equal
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public bool Equals(IDesign other)
        {
            return (ID == other.ID) &&
                   DesignDescriptor.Equals(other.Get_DesignDescriptor()) &&
                   (Extents.Equals(other.Extents));
        }

        /// <summary>
        /// Calculates an elevation subgrid for a desginatec subgrid on this design
        /// </summary>
        /// <param name="siteModelID"></param>
        /// <param name="originCellAddress"></param>
        /// <param name="cellSize"></param>
        /// <param name="designHeights"></param>
        /// <param name="errorCode"></param>
        /// <returns></returns>
        public bool GetDesignHeights(Guid siteModelID,
                                     ISubGridCellAddress originCellAddress,
                                     double cellSize,
                                     out IClientHeightLeafSubGrid designHeights,
                                     out DesignProfilerRequestResult errorCode)
        {
            // Query the DesignProfiler service to get the patch of elevations calculated
            errorCode = DesignProfilerRequestResult.OK;
            designHeights = null;

            try
            {
                designHeights = request.Execute(new CalculateDesignElevationPatchArgument()
                {
                    CellSize = cellSize,
                    DesignDescriptor = DesignDescriptor,
                    OriginX = originCellAddress.X,
                    OriginY = originCellAddress.Y,
                    // ProcessingMap = new SubGridTreeBitmapSubGridBits(SubGridBitsCreationOptions.Filled),
                    SiteModelID = siteModelID
                });
            }
            catch
            {
                errorCode = DesignProfilerRequestResult.UnknownError;
            }

            return errorCode == DesignProfilerRequestResult.OK;
        }
    }
}
