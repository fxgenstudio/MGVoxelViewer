// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace FxVoxelUtils
{
  public class GenMeshPart
  {
    public List<VertexPosColorNorm> m_vertices; // = new List<VertexPosColorNorm>();
    public List<ushort> m_indices;
  }


  /// <summary>
  /// Mesh Generator
  /// </summary>
  public class GenMesh
  {
    public MeshGPU m_meshGPU; //Created Mesh for GPU
    public bool m_bGPUDirty;  //True if m_meshGPU need to be rebuilded

    int m_vcount;  //Total Vertices
    int m_icount;  //Current Indices for current part
    int m_ncurPartIdx;

    public List<GenMeshPart> m_genMeshParts;  //One part per 65335 indices

    public BoundingBox m_bb;

    public GenMesh()
    {
      m_bGPUDirty = false;
      m_genMeshParts = new List<GenMeshPart>();
      m_ncurPartIdx = -1;

      AddNewPart();
    }


    /// <summary>
    /// Clear vertices and indices
    /// </summary>
    public void Clear()
    {
      m_bGPUDirty = true;

      m_vcount = 0;
      m_icount = 0;
      m_ncurPartIdx = -1;

      for (int i = 0; i < m_genMeshParts.Count; i++)
      {
        GenMeshPart part = m_genMeshParts[i];
        if (part.m_vertices != null) part.m_vertices.Clear();
        if (part.m_indices != null) part.m_indices.Clear();
      }

      m_genMeshParts.Clear();
      AddNewPart();
    }

    public int AddVertex(ref Vector3 _p, ref Color _c, ref Vector3 _norm)
    {
      m_vcount++;
      GenMeshPart part = m_genMeshParts[m_ncurPartIdx];
      part.m_vertices.Add(new VertexPosColorNorm(_p, _c, _norm));
      return part.m_vertices.Count - 1;
    }

    public void AddQuad(ushort _v0, ushort _v1, ushort _v2, ushort _v3)
    {
      GenMeshPart part = m_genMeshParts[m_ncurPartIdx];

      //Tri1
      part.m_indices.Add(_v0);
      part.m_indices.Add(_v1);
      part.m_indices.Add(_v2);
      //Tri2
      part.m_indices.Add(_v0);
      part.m_indices.Add(_v2);
      part.m_indices.Add(_v3);

      m_icount += 6;

      //Create New part if index buffer too big (>65536)
      if (m_icount > ushort.MaxValue - 6)
      {
        AddNewPart();
        
      }

    }

    private void AddNewPart()
    {
      GenMeshPart part = new GenMeshPart();
      part.m_vertices = new List<VertexPosColorNorm>();
      part.m_indices = new List<ushort>();
      m_genMeshParts.Add(part);  //New part
      m_ncurPartIdx++;
      m_icount = 0;
    }


    /// <summary>
    /// Queries the index of the current vertex. This starts at
    /// zero, and increments every time AddVertex is called.
    /// </summary>
    public int CurrentVertex
    {
      get { return m_vcount; }
    }

    /// <summary>
    /// Create a mesh objet for GPU
    /// </summary>
    /// <returns></returns>
    public void CreateGPUMesh(GraphicsDevice _gd, bool _bComputeBounds)
    {
      if (m_meshGPU == null)
        m_meshGPU = new MeshGPU();

      m_meshGPU.FromGenMesh(_gd, this, _bComputeBounds);

      m_bGPUDirty = false;

    }


  };

}
