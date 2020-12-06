using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace WoWFormatLib
{
    public class M2Track<T> where T : struct
    {
        public ushort InterpolationType;
        public short GlobalSequence;
        public uint[][] Timestamps;           // M2Array<M2Array<uint32>>
        public T[][] Values;                  // M2Array<M2Array<T>>

        public void Read(BinaryReader reader)
        {
            InterpolationType = reader.ReadUInt16();
            GlobalSequence = reader.ReadInt16();

            var timestampPair = reader.Read<M2Array>();
            var oldPos = reader.BaseStream.Position;
            reader.BaseStream.Position = timestampPair.Offset;

            //if (false) //Check for the version...
            //    reader.BaseStream.Position += 8;

            Timestamps = new uint[timestampPair.Count][];
            for (var i = 0; i < timestampPair.Count; ++i)
            {
                var timestamp = reader.Read<M2Array>();
                var oldPos_2 = reader.BaseStream.Position;

                reader.BaseStream.Position = timestamp.Offset;

                //if (false) //Check for the version...
                //    reader.BaseStream.Position += 8;

                Timestamps[i] = new uint[timestamp.Count];
                for (var j = 0; j < timestamp.Count; ++j)
                    Timestamps[i][j] = reader.ReadUInt32();

                reader.BaseStream.Position = oldPos_2;
            }
            reader.BaseStream.Position = oldPos;

            var valuePair = reader.Read<M2Array>();
            oldPos = reader.BaseStream.Position;
            reader.BaseStream.Position = valuePair.Offset;

            //if (false) //Check for the version...
            //    reader.BaseStream.Position += 8;

            Values = new T[valuePair.Count][];
            for (var i = 0; i < valuePair.Count; ++i)
            {
                var values = reader.Read<M2Array>();
                var oldPos_2 = reader.BaseStream.Position;

                reader.BaseStream.Position = values.Offset;

                //if (false) //Check for the version...
                //    reader.BaseStream.Position += 8;

                Values[i] = new T[values.Count];
                for (var j = 0; j < values.Count; ++j)
                    Values[i][j] = reader.Read<T>();

                reader.BaseStream.Position = oldPos_2;
            }
            reader.BaseStream.Position = oldPos;
        }
    }

    public struct M2Array
    {
        public uint Count;
        public uint Offset;
    }

    public static class Extensions
    {
        public static T Read<T>(this BinaryReader bin)
        {
            byte[] result = bin.ReadBytes(Unsafe.SizeOf<T>());
            return Unsafe.ReadUnaligned<T>(ref result[0]);
        }

        public static M2Track<T> ReadM2Track<T>(this BinaryReader reader) where T : struct
        {
            var m2Track = new M2Track<T>();
            m2Track.Read(reader);
            return m2Track;
        }

        /// <summary>
        ///  Reads the NULL terminated string from the current stream and advances the current position of the stream by string length + 1.
        /// <seealso cref="GenericReader.ReadStringNumber"/>
        /// </summary>
        public static string ReadStringNull(this BinaryReader reader)
        {
            byte num;
            string text = String.Empty;
            System.Collections.Generic.List<byte> temp = new System.Collections.Generic.List<byte>();

            while ((num = reader.ReadByte()) != 0)
                temp.Add(num);

            text = Encoding.UTF8.GetString(temp.ToArray());

            return text;
        }

        /// <summary>
        ///  Reads the string with known length from the current stream and advances the current position of the stream by string length.
        /// <seealso cref="GenericReader.ReadStringNull"/>
        /// </summary>
        public static string ReadStringNumber(this BinaryReader reader)
        {
            string text = String.Empty;
            uint num = reader.ReadUInt32(); // string length

            for (uint i = 0; i < num; i++)
            {
                text += (char)reader.ReadByte();
            }
            return text;
        }

    }
}