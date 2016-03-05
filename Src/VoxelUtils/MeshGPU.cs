// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


namespace FxVoxelUtils
{
  public class MeshGPUPart
  {
    public VertexBuffer m_gpuVertexBuffer;  //XNA buffer for DrawIndexedPrimitives
    public IndexBuffer m_gpuIndexBuffer;  //XNA buffer for DrawIndexedPrimitives
  }


  /// <summary>
  /// Mesh for renderer
  /// </summary>
  public class MeshGPU
  {

    #region -- Properties --
    public Effect SingleEffect
    {
      get { return m_effect; }
      set { m_effect = value; }
    }
    #endregion

    #region -- Fields --

    public Matrix m_worldMtx;
    public bool m_bUseWorldSpace; //True if view matrix is unused

    public Effect m_effect;

    public int m_npartsCount;
    public MeshGPUPart[] m_meshParts;

    public BoundingBox m_bb;
    public BoundingSphere m_bs;

    BasicEffect m_BBEffect;
    #endregion

    public MeshGPU()
    {
      m_meshParts = new MeshGPUPart[8]; //Default max ...

      for (int i = 0; i < m_meshParts.Length; i++)
        m_meshParts[i] = new MeshGPUPart();

      m_npartsCount = 0;
    }

    public void FromGenMesh(GraphicsDevice _gd, GenMesh _mesh, bool _bComputeBounds)
    {
      //Clear GPU Buffers
      ClearGPUVBOBuffers();

      //Init
      m_npartsCount = 0;



      for (int i = 0; i < _mesh.m_genMeshParts.Count; i++)
      {
        GenMeshPart part = _mesh.m_genMeshParts[i];
        MeshGPUPart partGPU = m_meshParts[i];

        //Vertices
        partGPU.m_gpuVertexBuffer = new VertexBuffer(_gd, typeof(VertexPosColorNorm), part.m_vertices.Count, BufferUsage.WriteOnly);
        partGPU.m_gpuVertexBuffer.SetData(part.m_vertices.ToArray(), 0, part.m_vertices.Count);

        //Indices
        partGPU.m_gpuIndexBuffer = new IndexBuffer(_gd, typeof(ushort), part.m_indices.Count, BufferUsage.WriteOnly);
        partGPU.m_gpuIndexBuffer.SetData(0, part.m_indices.ToArray(), 0, part.m_indices.Count);

        m_npartsCount++;
      }


    }


    public void ClearGPUVBOBuffers()
    {
      for (int i = 0; i < m_meshParts.Length; i++)
      {
        MeshGPUPart partGPU = m_meshParts[i];

        if (partGPU.m_gpuVertexBuffer != null)
        {
          partGPU.m_gpuVertexBuffer.Dispose();
          partGPU.m_gpuVertexBuffer = null;
        }

        if (partGPU.m_gpuIndexBuffer != null)
        {
          partGPU.m_gpuIndexBuffer.Dispose();
          partGPU.m_gpuIndexBuffer = null;
        }

      }


    }


    /// <summary>
    /// Frees resources used by this object.
    /// </summary>
    public void Dispose()
    {
      Dispose(true);
      GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees resources used by this object.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
      if (disposing)
      {
        ClearGPUVBOBuffers();
      }
    }



  }

}
