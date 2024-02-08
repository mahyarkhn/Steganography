using Microsoft.Win32.SafeHandles;
using System.Buffers.Binary;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Net.Security;
using System.Numerics;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace FileDataRW
{

    internal class Program
    {
        const int SECTOR_SIZE = 4096;
        static readonly string m_FileName = "\\PNG.png";
        static readonly string m_NewFileName = "\\NEW_PNG.png";
        static readonly string m_SrcFileName = "\\music.mp3";
        static readonly byte[] PNGSig = new byte[8]{ 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        static int offset = 0;
        static void Main(string[] args)
        {

            Console.WriteLine("Process Began...");
            if(!File.Exists(Path.Combine(Directory.GetCurrentDirectory() + m_FileName)))
            {
                Console.WriteLine("File \"PNG.png\" not found!");
                Console.ReadKey();
                Environment.Exit(-1);
            }

            byte[] buf = new byte[8];
            byte[] hbuf = new byte[4];
            string CType;
            int CSize = -1;
            bool bytesWritten = false;

            List<Chunk> chunks = new List<Chunk>();

            /*Stream originalFileStream = new FileStream(
                path: Path.Combine(Directory.GetCurrentDirectory() + m_FileName),
                mode: FileMode.Open,
                access: FileAccess.Read);*/

            /*Stream dstFileStream = new FileStream(
                path: Path.Combine(Directory.GetCurrentDirectory() + m_NewFileName),
                mode: FileMode.Create,
                access: FileAccess.ReadWrite
                );*/

            /*Stream srcFileStream = new FileStream(
                path: Path.Combine(Directory.GetCurrentDirectory() + m_SrcFileName),
                mode: FileMode.Open,
                access: FileAccess.Read
                );*/


            

            byte i = 0;
            int read = 0;
            byte[] sector = new byte[SECTOR_SIZE];

            using (Stream originalFileStream = File.OpenRead(Directory.GetCurrentDirectory() + m_FileName))    
            {
                if (originalFileStream.Length < 1) throw new Exception("Buffer is zero");
                originalFileStream.Read(buf, 0, 8);
                //if (!Buffer.Equals(PNGSig, buf)) throw new Exception("Signature doesnt match PNG");
                originalFileStream.Position = 8;
                //srcFileStream.Position = 0;
                using (Stream dstFileStream = File.OpenWrite(Directory.GetCurrentDirectory() + m_NewFileName))
                {
                    dstFileStream.Write(buf, 0, 8);
                    do
                    {
                        Chunk chunk = new Chunk { };
                        chunk.Offset = originalFileStream.Position;

                        // Read chunk data length
                        ReadChunk(originalFileStream, hbuf, 0, 4);
                        dstFileStream.Write(hbuf);
                        CSize = BinaryPrimitives.ReadInt32BigEndian(hbuf);
                        chunk.Size = CSize;

                        // Read chunk type
                        ReadChunk(originalFileStream, hbuf, 0, 4);
                        dstFileStream.Write(hbuf);
                        CType = Encoding.ASCII.GetString(hbuf);
                        chunk.Type = hbuf;

                        if (CType == "IDAT" && !bytesWritten)
                        {
                            originalFileStream.Seek(-8, SeekOrigin.Current);
                            dstFileStream.Seek(-8, SeekOrigin.Current);
                            using (Stream srcStream = File.OpenRead(path: Path.Combine(Directory.GetCurrentDirectory() + m_SrcFileName)))
                            {
                                dstFileStream.Write(BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(srcStream.Length)), 4, 4);
                                var typeBytes = Encoding.ASCII.GetBytes("MXDT");
                                dstFileStream.Write(typeBytes);
                                Crc32 crc32 = new Crc32();
                                crc32.SlidingWindow(typeBytes, 0, typeBytes.Length);
                                while ((read = srcStream.Read(sector)) > 0)//srcFileStream.Position < srcFileStream.Length)
                                {
                                    //byte @byte = (byte)srcFileStream.ReadByte();
                                    //srcFileStream.Read(sector);
                                    dstFileStream.Write(sector, 0, read);
                                    crc32.SlidingWindow(sector, 0, read);
                                }
                                dstFileStream.Write(BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(crc32.Hash)));
                                srcStream.Close();
                            }
                            bytesWritten = true;
                            continue;
                        }
                        uint xa = 0;

                        if (CSize > 0)
                        {
                            //read = 0;
                            //int _read = 0;
                            long chunkEnd = chunk.Offset + 8 + CSize;
                            //sector = Array.Empty<byte>();
                            //while ((read = originalFileStream.Read(sector, 0, sector.Length)) > 0)
                            //{
                            //    _read += read;
                            //    int count = ((int)(_read > chunkEnd ? originalFileStream.Position - chunkEnd + 1: read));
                            //    dstFileStream.Write(sector, 0, count);
                            //}
                            while (originalFileStream.Position < chunkEnd)
                            {
                                dstFileStream.WriteByte((byte)originalFileStream.ReadByte());
                            }
                            //ReadChunk(originalFileStream, b, 0, CSize);
                            //dstFileStream.Write(b);

                            //originalFileStream.Seek(CSize, SeekOrigin.Current);
                            //for(int x = CSize + 4; x >= 0 ; x--)
                            //{
                            //    int xb = originalFileStream.ReadByte();
                            //    originalFileStream.Position-=2;
                            //    xa = Crc32.Compute(Convert.ToByte(xb));
                            //}
                        }


                        // Read chunk crc32
                        //fs.Seek(CSize, SeekOrigin.Current);
                        ReadChunk(originalFileStream, hbuf, 0, 4);
                        dstFileStream.Write(hbuf);
                        chunk.CRC32 = hbuf;

                        //for(i = 0; i < CSize + 4; i++)
                        {
                            //Crc32.Compute()
                        }

                        Console.WriteLine(CSize + "|" + CType);

                        //chunks[i] = chunk;
                        //i++;
                        chunks.Add(chunk);

                        if (CType == "IDAT")
                        {
                            //fs.Seek(-CSize - 12, SeekOrigin.Current);
                        }
                    } while (CType != "IEND");
                }
            }

            //originalFileStream.Close();
            //originalFileStream.Dispose();
            //dstFileStream.Close();
            //dstFileStream.Dispose();
            //srcFileStream.Close();
            //srcFileStream.Dispose();

            

            Console.ReadKey();
            return;

            /*byte[] raw = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory() + m_FileName));
            Console.WriteLine("File data ready");
            Console.WriteLine("Press any key to continue");
            Console.ReadKey();

            RawData data = new RawData();
            data.Sz = 8;
            data.Data = ReadChunk(raw, offset, data.Sz);
            if (Buffer.Equals(data.Data, PNGSig))
            {
                Console.WriteLine("File Format is not PNG or either corrupt!");
                Console.ReadKey();
                Environment.Exit(-1);
            }
            offset = 8;
            Console.WriteLine("File format apporoved..");
            
            do
            {
                CSize = BinaryPrimitives.ReadInt32BigEndian(ReadChunk(raw, offset, 4));
                CType = Encoding.ASCII.GetString(ReadChunk(raw, offset + 4, 4));
                if(CType != "IDAT") offset += CSize + 12;
            } while (CType != "IDAT");
            Console.WriteLine(CType);


            byte[] musicraw = File.ReadAllBytes(Path.Combine(Directory.GetCurrentDirectory() + "\\music.mp3"));
            string payload_data = "Hello World";
            uint chunk_length = (uint)musicraw.Length; //(uint)payload_data.Length;
            Chunk chunk = new Chunk(chunk_length)
            {
                ChunkType = Encoding.ASCII.GetBytes("xPLD"),
                Data = musicraw//Encoding.ASCII.GetBytes(payload_data),
                //new byte[]{ 0x00, 0x00, 0x01, 0xA4, 0x00, 0x00, 0x01, 0x3B, 0x08, 0x06, 0x00, 0x00, 0x00 }//Encoding.ASCII.GetBytes("Hello World"),
            };


            Console.WriteLine(chunk.ComputeCrc32());
            byte[] newRaw = new byte[raw.Length + chunk.DataLength + 12];
            Buffer.BlockCopy(raw, 0, newRaw, 0, offset + 4);
            Buffer.BlockCopy(chunk.GetRaw(), 0, newRaw, offset, (int)chunk.DataLength + 12);
            int newOffset = offset + (int)chunk.DataLength + 12;
            Buffer.BlockCopy(raw, offset, newRaw, newOffset, raw.Length - offset);
            File.WriteAllBytes(Path.Combine(Directory.GetCurrentDirectory() + m_NewFileName), newRaw);

            //Console.WriteLine(Encoding.ASCII.GetString(chunk.GetRaw()));
            Console.ReadKey();*/

        }

        static int ReadChunk(Stream fileStream, byte[] buf, long offset, int size)
        {
            int readByte = 0, i;
            for (i = 0; i < size; i++)
            {
                readByte = fileStream.ReadByte();
                if (readByte < 0) break; // end of stream reached
                buf[i] = Convert.ToByte(readByte);
            }
            return i;
        }
        static void WriteChunk(Stream fileStream, byte[] buf, long offset, int size)
        {
            fileStream.Write(buf);

        }
        static byte[] ReadChunk(byte[] data, int start, int length)
        {
            byte[] bytes = new byte[length];
            Buffer.BlockCopy(data, start, bytes, 0, length);
            
            return bytes;
        }

        struct RawData
        {
            public byte[] Data;
            public int Sz;
        } 

        struct Chunk
        {
            public long Offset;
            public long Size;
            public byte[] Type = new byte[4];
            public byte[] CRC32 = new byte[4];

            public Chunk()
            {
            }
        }

        struct sChunk
        {
            public UInt32 DataLength;
            public long Offset = 0;
            public byte[] ChunkType = new byte[4];
            public byte[] Data;
            public byte[] CRC = new byte[4];
            private UInt32 crc;
            public sChunk(UInt32 dataLength)
            {
                DataLength = dataLength;
                Data = new byte[1];//new byte[dataLength];
            }

            /*public uint ComputeCrc32()
            {
                byte[] payload = new byte[ChunkType.Length + Data.Length];
                Buffer.BlockCopy(ChunkType, 0, payload, 0, ChunkType.Length);
                Buffer.BlockCopy(Data, 0, payload, ChunkType.Length, Data.Length);
                crc = Crc32.Compute(payload);
                CRC = BitConverter.GetBytes(crc);
                return crc;
            }*/

            /*public byte[] GetRaw()
            {
                byte[] raw = new byte[DataLength + 12];
                Buffer.BlockCopy(BitConverter.GetBytes(BinaryPrimitives.ReverseEndianness(DataLength)), 0, raw, 0, 4);
                Buffer.BlockCopy(ChunkType, 0, raw, 4, 4);
                Buffer.BlockCopy(Data, 0, raw, 8, (int)DataLength);
                Buffer.BlockCopy(CRC, 0, raw, (int)DataLength + 8, 4);
                return raw;
            }*/
        }
    }
}