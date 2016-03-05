// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using Microsoft.Xna.Framework;

namespace FxVoxelUtils
{
  public class CubeHelpers
  {
    public const int CUBE_SIZE = 2;

    //   y
    //   |
    //   |---x
    //  /
    // /z
    //

    /// <summary>
    /// Cube's faces side 
    /// </summary>
    public enum SIDE
    {
      INVALID = -1,
      O_LEFT = 0, //-x
      O_RIGHT,    //+x
      O_BOTTOM,   //-y
      O_TOP,      //+y
      O_BACK,     //-z
      O_FRONT     //+z
    };

    static public SIDE[] oppositeSide = new SIDE[]
    {
      SIDE.O_RIGHT,
      SIDE.O_LEFT,
      SIDE.O_TOP,
      SIDE.O_BOTTOM,
      SIDE.O_FRONT,
      SIDE.O_BACK
    };

    static public readonly Vector3Int[] SolidVertex =
    {
      new Vector3Int(0,0,0),
      new Vector3Int(1,0,0),
      new Vector3Int(0,1,0),
      new Vector3Int(1,1,0),
      new Vector3Int(0,0,1),
      new Vector3Int(1,0,1),
      new Vector3Int(0,1,1),
      new Vector3Int(1,1,1),
    };

    static public readonly int[,] fv = // indexes for voxelcoords, per face orientation
    {
      { 0, 2, 6, 4 },//-X
      { 5, 7, 3, 1 },//+X
      { 0, 4, 5, 1 },//-y
      { 6, 2, 3, 7 },//+y
      { 1, 3, 2, 0 },//-Z
      { 4, 6, 7, 5 },//+Z
    };



    static public readonly Vector3[] FacesPerSideNormal =  //Face normals
    {
      new Vector3(-1f, 0, 0),  //-X
      new Vector3( 1f, 0, 0),  //+X

      new Vector3( 0,-1f, 0),  //-Y
      new Vector3( 0, 1f, 0),  //+Y

      new Vector3( 0, 0, -1f),  //-Z
      new Vector3( 0, 0,  1f),  //+Z
    };

    // DIM:                            X  Y  Z
    static public readonly int[] R = { 1, 2, 1 }; // row
    static public readonly int[] C = { 2, 0, 0 }; // col
    static public readonly int[] D = { 0, 1, 2 }; // depth

    static public readonly Color[] dimColor =
    {
      Color.Red,    //X
      Color.Green,  //Y
      Color.Blue    //Z
    };

 

    public static void GetSolidWorldVertex(int _wcx, int _wcy, int _wcz, int _nNumVertex, out Vector3 _vtx)
    {
      var vtx = SolidVertex[_nNumVertex];
      _vtx.X = vtx.X + (float)_wcx;
      _vtx.Y = vtx.Y + (float)_wcy;
      _vtx.Z = vtx.Z + (float)_wcz;
    }




  }

}
