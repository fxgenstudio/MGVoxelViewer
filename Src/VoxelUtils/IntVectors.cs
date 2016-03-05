// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using System;

namespace FxVoxelUtils
{
  public struct Vector3Int
  {
    private const string THREE_COMPONENTS = "Array must contain exactly three components, (X,Y,Z)";

    public int X;
    public int Y;
    public int Z;


    public Vector3Int(int x, int y, int z)
    {
      this.X = x;
      this.Y = y;
      this.Z = z;
    }

    //value1 + (value2 - value1) * amount
    public static void Lerp(
             ref Vector3Int value1,
             ref Vector3Int value2,
             float amount,
             out Vector3Int result
    )
    {
      result.X = (int)(value1.X + (value2.X - value1.X) * amount);
      result.Y = (int)(value1.Y + (value2.Y - value1.Y) * amount);
      result.Z = (int)(value1.Z + (value2.Z - value1.Z) * amount);
    }
    

    public int this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: { return X; }
          case 1: { return Y; }
          case 2: { return Z; }
          default: throw new ArgumentException(THREE_COMPONENTS, "index");
        }
      }
      set
      {
        switch (index)
        {
          case 0: { X = value; break; }
          case 1: { Y = value; break; }
          case 2: { Z = value; break; }
          default: throw new ArgumentException(THREE_COMPONENTS, "index");
        }
      }
    }

    public override bool Equals(object obj)
    {
      if (obj is Vector3Int) return this.Equals((Vector3Int)obj);
      else return false;
    }

    public bool Equals(Vector3Int other)
    {
      return ((this.X == other.X) && (this.Y == other.Y) && (this.Z == other.Z));
    }

    public static bool operator ==(Vector3Int value1, Vector3Int value2)
    {
      return ((value1.X == value2.X) && (value1.Y == value2.Y)  && (value1.Z == value2.Z));
    }

    public static bool operator !=(Vector3Int value1, Vector3Int value2)
    {
      if (value1.X == value2.X && value1.Y == value2.Y) return value1.Z != value2.Z;
      return true;
    }

    public static Vector3Int operator *(Vector3Int value1, Vector3Int value2)
    {
      Vector3Int v = new Vector3Int();
      v.X = value1.X * value2.X;
      v.Y = value1.Y * value2.Y;
      v.Z = value1.Z * value2.Z;
      return v;
    }

    public static Vector3Int operator *(Vector3Int value1, int value2)
    {
      Vector3Int v = new Vector3Int();
      v.X = value1.X * value2;
      v.Y = value1.Y * value2;
      v.Z = value1.Z * value2;
      return v;
    }

    public static Vector3Int operator /(Vector3Int value1, int value2)
    {
      Vector3Int v = new Vector3Int();
      v.X = value1.X / value2;
      v.Y = value1.Y / value2;
      v.Z = value1.Z / value2;
      return v;
    }

    public override int GetHashCode()
    {
      return (this.X.GetHashCode() + this.Z.GetHashCode());
    }

    public override string ToString()
    {
      return string.Format("{{X:{0} Y:{1} Z:{2}}}", this.X, this.Y, this.Z);
    }
  }

  public struct Vector2Int
  {
    private const string TWO_COMPONENTS = "Array must contain exactly two components, (X,Y)";

    public int X;
    public int Y;

    public Vector2Int(int x, int y)
    {
      this.X = x;
      this.Y = y;
    }

    //value1 + (value2 - value1) * amount
    public static void Lerp(
             ref Vector2Int value1,
             ref Vector2Int value2,
             float amount,
             out Vector2Int result
    )
    {
      result.X = (int)(value1.X + (value2.X - value1.X) * amount);
      result.Y = (int)(value1.Y + (value2.Y - value1.Y) * amount);
    }


    public int this[int index]
    {
      get
      {
        switch (index)
        {
          case 0: { return X; }
          case 1: { return Y; }
          default: throw new ArgumentException(TWO_COMPONENTS, "index");
        }
      }
      set
      {
        switch (index)
        {
          case 0: { X = value; break; }
          case 1: { Y = value; break; }
          default: throw new ArgumentException(TWO_COMPONENTS, "index");
        }
      }
    }

    public override bool Equals(object obj)
    {
      if (obj is Vector2Int) return this.Equals((Vector2Int)obj);
      else return false;
    }

    public bool Equals(Vector2Int other)
    {
      return ((this.X == other.X) && (this.Y == other.Y));
    }

    public static bool operator ==(Vector2Int value1, Vector2Int value2)
    {
      return ((value1.X == value2.X) && (value1.Y == value2.Y));
    }

    public static bool operator !=(Vector2Int value1, Vector2Int value2)
    {
      if (value1.X == value2.X ) return value1.Y != value2.Y;
      return true;
    }

    public static Vector2Int operator *(Vector2Int value1, Vector2Int value2)
    {
      Vector2Int v = new Vector2Int();
      v.X = value1.X * value2.X;
      v.Y = value1.Y * value2.Y;
      return v;
    }

    public static Vector2Int operator -(Vector2Int value1, Vector2Int value2)
    {
      Vector2Int v = new Vector2Int();
      v.X = value1.X - value2.X;
      v.Y = value1.Y - value2.Y;
      return v;
    }
    public static Vector2Int operator *(Vector2Int value1, int value2)
    {
      Vector2Int v = new Vector2Int();
      v.X = value1.X * value2;
      v.Y = value1.Y * value2;
      return v;
    }

    public static Vector2Int operator /(Vector2Int value1, int value2)
    {
      Vector2Int v = new Vector2Int();
      v.X = value1.X / value2;
      v.Y = value1.Y / value2;
      return v;
    }

    public int Dot(Vector2Int _o)
    {
      return (X * _o.X + Y * _o.Y);
    }

    public int Cross(Vector2Int _o)
    {
      return (X * _o.Y - Y * _o.X);
    }

    public override int GetHashCode()
    {
      return (this.X.GetHashCode() + this.Y.GetHashCode());
    }

    public override string ToString()
    {
      return string.Format("{{X:{0} Y:{1}}}", this.X, this.Y);
    }
  }




}
