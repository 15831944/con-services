﻿using System;
using System.IO;
using System.Text;
using VSS.TRex.Common.Interfaces;
using VSS.TRex.IO.Helpers;

namespace VSS.TRex.Common.Utilities.ExtensionMethods
{
  /// <summary>
  /// Extension methods supporting serialisation and deserialization to and from vanilla byte arrays.
  /// </summary>
  public static class FromToBytes
  {
    /*  An example that requires static extension methods to work...
            public static T FromBytes<T>(this T item, byte[] bytes) where T : class, IBinaryReaderWriter, new()
            {
                using (MemoryStream ms = new MemoryStream(bytes))
                {
                    using (BinaryReader reader = new BinaryReader(ms))
                    {
                        T newItem = new T();
                        newItem.Read(reader);
                        return newItem;
                    }
                }
            }
    */

    /// <summary>
    /// An extension method providing a FromBytes() semantic to deserialize a byte array via the class defined Read() implementation
    /// </summary>
    public static void FromBytes<T>(this T item, byte[] bytes) where T : class, IBinaryReaderWriter => FromBytes(bytes, item.Read);

    /// <summary>
    /// An extension method providing a FromBytes() semantic to deserialize a stream via the class defined Read() implementation
    /// </summary>
    public static void FromStream<T>(this T item, Stream stream) where T : class, IBinaryReaderWriter => FromStream(stream, item.Read);

    /// <summary>
    /// An extension method providing a ToStream() semantic to serialise its state to a stream via the class defined Write() implementation
    /// </summary>
    public static byte[] ToBytes<T>(this T item) where T : class, IBinaryReaderWriter => ToBytes(item.Write);

    /// <summary>
    /// An extension method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    public static MemoryStream ToStream<T>(this T item) where T : class, IBinaryReaderWriter => ToStream(item.Write);

    /// <summary>
    /// A generic method providing a ToBytes() semantic to serialise its state to a byte array via the class defined Write() implementation
    /// </summary>
    public static byte[] ToBytes(Action<BinaryWriter> serializer)
    {
      using (var ms = RecyclableMemoryStreamManagerHelper.Manager.GetStream())
      {
        using (var writer = new BinaryWriter(ms))
        {
          serializer(writer);
          return ms.ToArray();
        }
      }
    }

    /// <summary>
    /// A generic method providing a ToStream() semantic to serialise its state to a stream via the class defined Write() implementation
    /// </summary>
    public static MemoryStream ToStream(Action<BinaryWriter> serializer)
    {
      var ms = RecyclableMemoryStreamManagerHelper.Manager.GetStream();
      {
        using (var writer = new BinaryWriter(ms, Encoding.UTF8, true))
        {
          serializer(writer);
        }
      }
      return ms;
    }

    /// <summary>
    /// A generic method providing a ToStream() semantic to serialise its state to a stream via the class defined Write() implementation
    /// </summary>
    public static void ToStream(Stream stream, Action<BinaryWriter> serializer)
    {
      using (BinaryWriter writer = new BinaryWriter(stream, Encoding.UTF8, true))
      {
        serializer(writer);
      }
    }

    /// <summary>
    /// A generic providing a FromBytes() semantic to deserialize a byte array via the class defined Read() implementation
    /// </summary>
    public static void FromBytes(byte[] bytes, Action<BinaryReader> deserializer)
    {
      using (MemoryStream ms = new MemoryStream(bytes))
      {
        using (BinaryReader reader = new BinaryReader(ms))
        {
          deserializer(reader);
        }
      }
    }

    /// <summary>
    /// An extension method providing a FromStream() semantic to deserialize a stream via the class defined Read() implementation
    /// </summary>
    public static void FromStream(Stream stream, Action<BinaryReader> deserializer)
    {
      using (BinaryReader reader = new BinaryReader(stream, Encoding.UTF8, true))
      {
        deserializer(reader);
      }
    }
  }
}
