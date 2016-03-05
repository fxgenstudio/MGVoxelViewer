// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using System.Diagnostics;

namespace FxVoxelUtils
{

  /// <summary>
  /// Voxel Description
  /// </summary>
  public struct SmallCube
  {
    public byte byMatL0;
  }

  ////////////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// Chunk composed of Cubes (or Voxels)
  /// </summary>
  ////////////////////////////////////////////////////////////////////////////////////
  public class CubeChunk3D
  {
    public SmallCube[] m_array; //If Sparse

    public SmallCube m_solidChunkCube; //If solid Chunk


    public CubeChunk3D()
    {
      m_solidChunkCube = CubeArray3D.m_emptyCube;
    }

    public void GetCube(int _x, int _y, int _z, out SmallCube _c)
    {
      if (m_array == null)
        _c = m_solidChunkCube;
      else
        _c = m_array[(_x << CubeArray3D.FlattenOffsetChunk_OPSHIFT) + (_z << CubeArray3D.CHUNKSIZE_OPSHIFT) + _y];
    }

    public void SetCube(int _x, int _y, int _z, SmallCube _c)
    {
      if (m_array == null)
        m_array = new SmallCube[CubeArray3D.CHUNKSIZE * CubeArray3D.CHUNKSIZE * CubeArray3D.CHUNKSIZE];

      m_array[(_x << CubeArray3D.FlattenOffsetChunk_OPSHIFT) + (_z << CubeArray3D.CHUNKSIZE_OPSHIFT) + _y] = _c;
    }




  }

  ////////////////////////////////////////////////////////////////////////////////////
  /// <summary>
  /// One CubeArray3D contain chunks of size CHUNKSIZE*CHUNKSIZE*CHUNKSIZE
  /// CubeArray3D size must be CHUNKSIZE modulo
  /// </summary>
  ////////////////////////////////////////////////////////////////////////////////////
  public class CubeArray3D
  {
    public int CUBESIZEX;
    public int CUBESIZEY;
    public int CUBESIZEZ;

    public static SmallCube m_emptyCube = new SmallCube();

    public int CHUNKSIZEX; // is CUBESIZEX / CHUNKSIZE;
    public int CHUNKSIZEY; // is CUBESIZEY / CHUNKSIZE;
    public int CHUNKSIZEZ; // is CUBESIZEZ / CHUNKSIZE;

    public const int CHUNKSIZE_OPSHIFT = 4; //div 16
    public const int CHUNKSIZE = 16; //chunk size is a cube !

    public const int FlattenOffsetChunk_OPSHIFT = 8; //m_arrayChunks[(x * (sy*sz)) + (z * sy) + y]

    int FlattenOffset;  // = (BLOCKSIZEY * BLOCKSIZEZ); //m_arrayChunks[(x * (sy*sz)) + (z * sy) + y]

    CubeChunk3D[] m_arrayChunks;


    public void SetSize(int _sizeX, int _sizeY, int _sizeZ)
    {
      if (CUBESIZEX == _sizeX && CUBESIZEY == _sizeY && CUBESIZEZ == _sizeZ)
        return;

      Debug.Assert(_sizeX % CHUNKSIZE == 0);
      Debug.Assert(_sizeY % CHUNKSIZE == 0);
      Debug.Assert(_sizeZ % CHUNKSIZE == 0);

      CUBESIZEX = _sizeX;
      CUBESIZEY = _sizeY;
      CUBESIZEZ = _sizeZ;

      CHUNKSIZEX = _sizeX >> CHUNKSIZE_OPSHIFT; //Chunks Count in X
      CHUNKSIZEY = _sizeY >> CHUNKSIZE_OPSHIFT; //Chunks Count in Y
      CHUNKSIZEZ = _sizeZ >> CHUNKSIZE_OPSHIFT; //Chunks Count in Z

      m_arrayChunks = new CubeChunk3D[CHUNKSIZEX * CHUNKSIZEY * CHUNKSIZEZ];

      FlattenOffset = (CHUNKSIZEY * CHUNKSIZEZ); //m_arrayChunks[(x * (sy*sz)) + (z * sy) + y]
    }

    static public CubeArray3D ReSize(CubeArray3D _array, int _newsizeX, int _newsizeY, int _newsizeZ)
    {
      Debug.Assert(_newsizeX != _array.CUBESIZEX || _newsizeY == _array.CUBESIZEY || _newsizeZ == _array.CUBESIZEZ);

      CubeArray3D newCubeArray = new CubeArray3D();
      newCubeArray.SetSize(_newsizeX, _newsizeY, _newsizeZ);

      int middleX = (_array.CUBESIZEX >> 1);
      int middleY = (_array.CUBESIZEY >> 1);
      int middleZ = (_array.CUBESIZEZ >> 1);

      int newmiddleX = (_newsizeX >> 1);
      int newmiddleY = (_newsizeY >> 1);
      int newmiddleZ = (_newsizeZ >> 1);

      SmallCube c;
      int x, y, z;
      int x2, y2, z2;
      for (x = 0; x < _array.CUBESIZEX; x++)
      {
        for (z = 0; z < _array.CUBESIZEZ; z++)
        {
          for (y = 0; y < _array.CUBESIZEY; y++)
          {
            x2 = (x - middleX) + newmiddleX;
            y2 = (y - middleY) + newmiddleY;
            z2 = (z - middleZ) + newmiddleZ;

            if (x2 < 0 || y2 < 0 || z2 < 0) continue;
            if (x2 >= _newsizeX || y2 >= _newsizeY || z2 >= _newsizeZ) continue;

            _array.GetCube(x, y, z, out c);  //Get Source
            if (c.byMatL0 != 0)
            {
              //Set destination
              newCubeArray.SetCube(x2, y2, z2, c);
            }


          }
        }
      }

      //Return new buffer
      return newCubeArray;
    }

    static public void Copy(CubeArray3D _src, CubeArray3D _dst)
    {
      _dst.Clear();
      _dst.SetSize(_src.CUBESIZEX, _src.CUBESIZEY, _src.CUBESIZEZ);

      SmallCube c;
      int x, y, z;
      for (x = 0; x < _src.CUBESIZEX; x++)
      {
        for (z = 0; z < _src.CUBESIZEZ; z++)
        {
          for (y = 0; y < _src.CUBESIZEY; y++)
          {
            _src.GetCube(x, y, z, out c);  //Get Source
            _dst.SetCube(x, y, z, c); //Set Destination
          }
        }
      }
    }

    /// <summary>
    /// Clear all chunks
    /// </summary>
    public void Clear()
    {
      int x, y, z;
      int offset;

      for (x = 0; x < CHUNKSIZEX; x++)
      {
        for (z = 0; z < CHUNKSIZEZ; z++)
        {
          for (y = 0; y < CHUNKSIZEY; y++)
          {
            offset = (x * FlattenOffset) + (z * CHUNKSIZEY) + y;
            m_arrayChunks[offset] = null;
          }
        }
      }
    }


    public CubeChunk3D GetChunk(int chX, int chY, int chZ)
    {
      int offset = (chX * FlattenOffset) + (chZ * CHUNKSIZEY) + chY;
      return m_arrayChunks[offset];
    }

    /// <summary>
    /// Set Cube value at coords
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="st"></param>
    public void SetCube(int x, int y, int z, SmallCube st)
    {
      int chX = x >> CHUNKSIZE_OPSHIFT;  //x / CHUNKSIZE;
      int chY = y >> CHUNKSIZE_OPSHIFT;  //y / CHUNKSIZE;
      int chZ = z >> CHUNKSIZE_OPSHIFT;  //z / CHUNKSIZE;
      int offset = (chX * FlattenOffset) + (chZ * CHUNKSIZEY) + chY;
      CubeChunk3D chunk = m_arrayChunks[offset];
      if (chunk == null)
      {
        chunk = new CubeChunk3D();
        m_arrayChunks[offset] = chunk;
      }

      chX = x - (chX << CHUNKSIZE_OPSHIFT); //x - (bx * CHUNKSIZE);
      chY = y - (chY << CHUNKSIZE_OPSHIFT); //y - (by * CHUNKSIZE);
      chZ = z - (chZ << CHUNKSIZE_OPSHIFT); //z - (bz * CHUNKSIZE);

      chunk.SetCube(chX, chY, chZ, st);

    }

    /// <summary>
    /// Get Cube vvalue at coords
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="_c"></param>
    public void GetCube(int x, int y, int z, out SmallCube _c)
    {
      //Get chunk
      int chX = x >> CHUNKSIZE_OPSHIFT;  //x / CHUNKSIZE;
      int chY = y >> CHUNKSIZE_OPSHIFT;  //y / CHUNKSIZE;
      int chZ = z >> CHUNKSIZE_OPSHIFT;  //z / CHUNKSIZE;
      int offset = (chX * FlattenOffset) + (chZ * CHUNKSIZEY) + chY;

      CubeChunk3D chunk = m_arrayChunks[offset];
      if (chunk == null)
      {
        _c = m_emptyCube;
        return;
      }

      if (chunk.m_array == null)
      {
        _c = chunk.m_solidChunkCube;
        return;
      }

      //Return Cube into chunk
      chX = x - (chX << CHUNKSIZE_OPSHIFT); //x - (bx * CHUNKSIZE);
      chY = y - (chY << CHUNKSIZE_OPSHIFT); //(by * CHUNKSIZE);
      chZ = z - (chZ << CHUNKSIZE_OPSHIFT); //(bz * CHUNKSIZE);

      offset = (chX << FlattenOffsetChunk_OPSHIFT) + (chZ << CHUNKSIZE_OPSHIFT) + chY;

      _c = chunk.m_array[offset];
    }

    public bool GetNeighbourCube(int x, int y, int z, CubeHelpers.SIDE side, out SmallCube _c)
    {
      _c = m_emptyCube;

      switch (side)
      {
        case CubeHelpers.SIDE.O_LEFT: x -= 1; break;  //0: -x
        case CubeHelpers.SIDE.O_RIGHT: x += 1; break; //1: +x
        case CubeHelpers.SIDE.O_BOTTOM: y -= 1; break;//2: -y
        case CubeHelpers.SIDE.O_TOP: y += 1; break;   //3: +y
        case CubeHelpers.SIDE.O_BACK: z -= 1; break;  //4: -z
        case CubeHelpers.SIDE.O_FRONT: z += 1; break; //5: +z
      }
      if (x < 0 || y < 0 || z < 0) return false;
      if (x >= CUBESIZEX || y >= CUBESIZEY || z >= CUBESIZEZ) return false;

      GetCube(x, y, z, out _c);
      return true;
    }



  }


}
