﻿namespace VSS.TRex.Designs.SVL
{
  public static class NFFConsts
  {
    // The following flags relate to the header flags for every entity in the NFF file
    // For v1.5 and later NFF file they are stored directly in the header of an entity
    // in their own byte. For pre v1.5 files
    public static byte kNFFElementHeaderHasElevation  = 0x1;
    public static byte kNFFElementHeaderHasStationing = 0x2;
    public static byte kNFFElementHeaderHasGuidanceID = 0x4;
    public static byte kNFFElementHeaderHasCrossSlope = 0x8;

    // The following flags are used in the record type byte at the front of
    // every entity in an NFF file (applies to pre v1.5 version NFF files.
    public static byte kNFFHasHeight = 0x20;
    public static byte kNFFHasStationing = 0x40;
    public static byte kNFFHasGuidanceID = 0x80;

    // kNFFNullCrossSlopeValue is the value defined by the machine control system to mean
    // a null cross slope value. PCS900 will then interpolate cross slope values between
    // bracketing non-null cross slope values.
    public static float kNFFNullCrossSlopeValue = 1e38F;

    // The following flags relate to guidance alignments
    public static byte kNFFGuidanceIDHasStationing = 0x1;
    public static byte kNFFGuidanceIDHasStationRange = 0x2;
    public static byte kNFFGuidanceIDHasElevation = 0x4;
    public static byte kNFFGuidanceIDHasCrossSlope = 0x8;

    public const int kNFFMagicNumberArraySize = 8;

    public const string kNFFSurfaceFileMagicNumber = "TRMSRFCE";
    public const string kNFFLineworkFileMagicNumber = "TRMLINES";
    public const string kNFFIndexFileMagicNumber = "TRMINDEX";

    public const string kNFFIndexStorageName = "Header";
  }
}
