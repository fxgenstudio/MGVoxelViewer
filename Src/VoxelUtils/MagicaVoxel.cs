// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace FxVoxelUtils
{

    /// <summary>
    /// Voxel Desc
    /// </summary>
    public struct VoxelDesc
    {
        public int x, y, z, i;
    }

    /// <summary>
    /// MagicaVoxel model Loader
    /// https://ephtracy.github.io/index.html?page=mv_vox_format
    /// </summary>
    public class MagicaVoxelLoader
    {
        Color[] m_palette;
        public Color[] Palette { get { return m_palette; } }

        CubeArray3D m_carray;
        public CubeArray3D Array { get { return m_carray; } }


        public MagicaVoxelLoader()
        {
            m_carray = new CubeArray3D();
            m_palette = new Color[256];
        }

        /// <summary>
        /// Read .VOX File and return a CubeArray3D
        /// </summary>
        /// <param name="_strPath"></param>
        /// <returns></returns>
        //public bool ReadFile(Windows.Storage.Streams.DataReader _br)
        public bool ReadFile(BinaryReader _br)
        {
            //////////////////////////////////////////////////
            //Read Header
            //4 bytes: magic number ('V' 'O' 'X' 'space' )
            //4 bytes: version number (current version is 150 )
            UInt32 signature = _br.ReadUInt32();
            if (signature != 0x20584F56)  //56 4F 58 20
            {
                Debug.WriteLine("Not an MagicaVoxel File format");
                return false;
            }

            UInt32 version = _br.ReadUInt32();
            if (version < 150)
            {
                Debug.WriteLine("MagicaVoxel version too old");
                return false;
            }


            // header
            //4 bytes: chunk id
            //4 bytes: size of chunk contents (n)
            //4 bytes: total size of children chunks(m)

            //// chunk content
            //n bytes: chunk contents

            //// children chunks : m bytes
            //{ child chunk 0 }
            //{ child chunk 1 }
            int sizeX, sizeY, sizeZ;
            sizeX = sizeY = sizeZ = 0;
            int numVoxels = 0;
            int offsetX, offsetY, offsetZ;
            offsetX = offsetY = offsetZ = 0;

            SmallCube cube = new SmallCube();

#if ANDROID
            while (_br.BaseStream.IsDataAvailable())
#else
            while (_br.BaseStream.Position < _br.BaseStream.Length)
#endif
            {

                string chunkName = new string(_br.ReadChars(4));
                UInt32 chunkSize = _br.ReadUInt32();
                UInt32 chunkTotalChildSize = _br.ReadUInt32();


                if (chunkName == "SIZE")
                {
                    //(4 bytes x 3 : x, y, z ) 
                    sizeX = _br.ReadInt32();
                    sizeY = _br.ReadInt32();
                    sizeZ = _br.ReadInt32();

                    //Align size to chunk size
                    int sx = sizeX + ((CubeArray3D.CHUNKSIZE - (sizeX % CubeArray3D.CHUNKSIZE)) % CubeArray3D.CHUNKSIZE);
                    int sy = sizeY + ((CubeArray3D.CHUNKSIZE - (sizeY % CubeArray3D.CHUNKSIZE)) % CubeArray3D.CHUNKSIZE);
                    int sz = sizeZ + ((CubeArray3D.CHUNKSIZE - (sizeZ % CubeArray3D.CHUNKSIZE)) % CubeArray3D.CHUNKSIZE);

                    m_carray.SetSize(sx, sz, sy); //Reversed y-z

                    offsetX = (sx - sizeX) >> 1;
                    offsetY = (sz - sizeZ) >> 1;//Reversed y-z
                    offsetZ = (sy - sizeY) >> 1;//Reversed y-z

                }
                else if (chunkName == "XYZI")
                {
                    //(numVoxels : 4 bytes )
                    //(each voxel: 1 byte x 4 : x, y, z, colorIndex ) x numVoxels
                    numVoxels = _br.ReadInt32();
                    while (numVoxels > 0)
                    {
                        byte vx = _br.ReadByte();
                        byte vy = _br.ReadByte();
                        byte vz = _br.ReadByte();
                        byte vi = _br.ReadByte();
                        cube.byMatL0 = vi;
                        m_carray.SetCube(offsetX + vx, offsetY + vz, m_carray.CUBESIZEZ - vy - 1 - offsetZ, cube);  //Reserved y-z

                        numVoxels--;
                    }
                }
                else if (chunkName == "RGBA")
                {
                    //(each pixel: 1 byte x 4 : r, g, b, a ) x 256
                    for (int i = 0; i < 256; i++)
                    {
                        byte r = _br.ReadByte();
                        byte g = _br.ReadByte();
                        byte b = _br.ReadByte();
                        byte a = _br.ReadByte();

                        m_palette[i] = Color.FromNonPremultiplied(r, g, b, a);
                    }



                }
            }

            return true;
        }

    }


    /// <summary>
    /// MagicVoxel model Saver
    /// </summary>
    public class MagicaVoxelSaver
    {
        CubeArray3D m_array;

        public MagicaVoxelSaver()
        {
            m_array = null;
        }


        public bool WriteFile(BinaryWriter _bw, CubeArray3D _array, Color[] _palette)
        {
            int i;

            //Make voxel array
            var list = MakeVoxelsList(_array);

            //////////////////////////////////////////////////
            //Write Header
            //4 bytes: magic number ('V' 'O' 'X' 'space' )
            //4 bytes: version number (current version is 150 )
            _bw.Write((UInt32)0x20584F56);
            _bw.Write((UInt32)150);


            //////////////////////////////////////////////////
            // header
            //4 bytes: chunk id
            //4 bytes: size of chunk contents (n)
            //4 bytes: total size of children chunks(m)

            //// chunk content
            //n bytes: chunk contents

            //// children chunks : m bytes
            //{ child chunk 0 }
            //{ child chunk 1 }

            ///////////////////////////////////////////////////
            //Chunk MAIN
            UInt32 chunkLen_SIZE = 4 * 3;
            UInt32 chunkLen_XYZI = (UInt32)(list.Count * 4) + 4;
            UInt32 chunkLen_RGBA = (UInt32)(256 * 4);

            UInt32 chunkTotalChildSize = chunkLen_SIZE + chunkLen_XYZI + chunkLen_RGBA + (4 * 3 * 3);

            _bw.Write(new char[] { 'M', 'A', 'I', 'N' }); //chunkName

            _bw.Write((UInt32)0); //chunkSize
            _bw.Write(chunkTotalChildSize);   //chunkTotalChildSize = 0 ?????


            ///////////////////////////////////////////////////
            //Chunk SIZE
            _bw.Write(new char[] { 'S', 'I', 'Z', 'E' }); //chunkName

            _bw.Write(chunkLen_SIZE); //chunkSize = (4 bytes x 3 : x, y, z ) 
            _bw.Write((UInt32)0);   //chunkTotalChildSize = 0

            _bw.Write((Int32)m_array.CUBESIZEX);
            _bw.Write((Int32)m_array.CUBESIZEZ);//Reversed y-z
            _bw.Write((Int32)m_array.CUBESIZEY);//Reversed y-z

            ///////////////////////////////////////////////////
            //Chunk XYZI
            _bw.Write(new char[] { 'X', 'Y', 'Z', 'I' });

            _bw.Write(chunkLen_XYZI); //chunkSize
            _bw.Write((UInt32)0);   //chunkTotalChildSize = 0

            //(numVoxels : 4 bytes )
            _bw.Write(list.Count);

            //(each voxel: 1 byte x 4 : x, y, z, colorIndex ) x numVoxels
            for (i = 0; i < list.Count; i++)
            {
                var vox = list[i];
                _bw.Write((byte)vox.x);
                _bw.Write((byte)(m_array.CUBESIZEZ - vox.z - 1));//Reversed y-z
                _bw.Write((byte)vox.y);//Reversed y-z
                _bw.Write((byte)vox.i);
            }

            ///////////////////////////////////////////////////
            //Chunk RGBA
            _bw.Write(new char[] { 'R', 'G', 'B', 'A' });
            _bw.Write(chunkLen_RGBA); //chunkSize
            _bw.Write((UInt32)0);   //chunkTotalChildSize = 0

            //(each pixel: 1 byte x 4 : r, g, b, a ) x 256
            for (i = 0; i < 256; i++)
            {
                var col = _palette[i]; // first color ( at position 0 ) is corresponding to color index 1.
                _bw.Write((byte)col.R);
                _bw.Write((byte)col.G);
                _bw.Write((byte)col.B);
                _bw.Write((byte)col.A);
            }


            return true;
        }

        private List<VoxelDesc> MakeVoxelsList(CubeArray3D _array)
        {
            List<VoxelDesc> list = new List<VoxelDesc>();
            SmallCube cube;
            int x, y, z;
            for (x = 0; x < _array.CUBESIZEX; x++)
            {
                for (y = 0; y < _array.CUBESIZEY; y++)
                {
                    for (z = 0; z < _array.CUBESIZEZ; z++)
                    {
                        _array.GetCube(x, y, z, out cube);
                        if (cube.byMatL0 != 0)
                        {
                            var vd = new VoxelDesc();
                            vd.x = x; vd.y = y; vd.z = z;
                            vd.i = (int)cube.byMatL0;
                            list.Add(vd);
                        }
                    }
                }
            }
            return list;
        }
    }


}
