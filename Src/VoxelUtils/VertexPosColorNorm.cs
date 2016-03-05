// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace FxVoxelUtils
{

  public struct VertexPosColorNorm : IVertexType
  {
    public Vector3 Pos;
    public Color Col;
    public Vector3 Norm;

    /// <summary>
    /// Constructor.
    /// </summary>
    public VertexPosColorNorm(Vector3 pos, Color col, Vector3 norm)
    {
      Pos = pos;
      Col = col;
      Norm = norm;
    }

    /// <summary>
    /// A VertexDeclaration object, which contains information about the vertex
    /// elements contained within this struct.
    /// </summary>
    public static readonly VertexDeclaration VertexDeclaration = new VertexDeclaration
    (
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
        new VertexElement(16, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0)
    );

    VertexDeclaration IVertexType.VertexDeclaration
    {
      get { return VertexPosColorNorm.VertexDeclaration; }
    }

  }


}
