// MIT License - Copyright (C) FxGen 
// This file is subject to the terms and conditions defined in 
// file 'LICENSE.txt', which is part of this source code package. 

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;

namespace FxVoxelUtils
{
    public partial class CubeBrush
    {
        #region -- Fields --
        public CubeArray3D m_array;
        public Color[] m_colPalette;  //256 colors palette per brush

        GenMesh m_genMesh;
        bool[] m_bVertexComputed = new bool[8]; //For each vertices
        Vector3[] m_vertComputed = new Vector3[8];
        Color[] m_vertCols = new Color[24];
        Color[] m_shadowVerts = new Color[4];

        Texture2D m_tex2DBorder;

        public Vector3 m_vec3Origin;//Accessed by Edit

        public Matrix m_worldMtx; //Accessed by Edit
        public Vector2 m_manipAngles;//Accessed by Edit
        public Vector3Int m_vec3CubeOrigin;//Accessed by Edit for Miror

        public const int AO_CUBESIZE_X = CubeArray3D.CHUNKSIZE + (2 + 2);
        public const int AO_CUBESIZE_Y = CubeArray3D.CHUNKSIZE + (2 + 2);
        public const int AO_CUBESIZE_Z = CubeArray3D.CHUNKSIZE + (2 + 2);
        public const int FlattenOffsetAO = (AO_CUBESIZE_Y * AO_CUBESIZE_Z); //3darray[(x * (sy*sz)) + (z * sy) + y]
        static protected byte[] m_abyOcclusionCubeMap = new byte[AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z]; //Array for occlusion

        Vector3 LightDir = new Vector3(0.0f, 0.0f, -1.0f);

        #endregion

        #region -- Properties --
        public string UserName { get; set; }
        public GenMesh GenMesh { get { return m_genMesh; } }

        public bool ShowGridOnVoxel { get; set; }

        public Game Game { get; set; }

        #endregion

        #region -- ctor --
        public CubeBrush(Game _game, int _sizeX, int _sizeY, int _sizeZ, bool _createDummy)
        {
            Game = _game;

            //Array size
            SetSize(_sizeX, _sizeY, _sizeZ);

            //Create a plane at bottom
            if (_createDummy)
            {
                SmallCube cube = new SmallCube();
                cube.byMatL0 = 16;

                int x, y, z;
                y = 0;
                for (x = 0; x < _sizeX; x++)
                {
                    for (z = 0; z < _sizeZ; z++)
                    {
                        m_array.SetCube(x, y, z, cube);
                    }
                }

            }

            //Create Palette
            BuildDefaultColorPalette();

            //Meshes
            m_genMesh = new GenMesh();

        }

        public CubeBrush(Game _game, CubeArray3D _carray, Color[] _palette)
        {
            Game = _game;


            //Set Palette
            m_colPalette = _palette;

            //Set Array
            m_array = _carray;
            InitFromNewArray();

            //Meshes
            m_genMesh = new GenMesh();

        }

        #endregion

        #region -- Edition --

        public void SetSize(int _sizeX, int _sizeY, int _sizeZ)
        {
            //Check if size have changed
            if (m_array != null && (_sizeX == m_array.CUBESIZEX && _sizeY == m_array.CUBESIZEY && _sizeZ == m_array.CUBESIZEZ))
                return;

            //3D Array
            if (m_array == null)
            {
                m_array = new CubeArray3D();
                m_array.SetSize(_sizeX, _sizeY, _sizeZ);
            }
            else
            {
                m_array = CubeArray3D.ReSize(m_array, _sizeX, _sizeY, _sizeZ);
            }

            //Init
            InitFromNewArray();

        }

        public void ApplyEditorEffect()
        {

            if (m_genMesh.m_meshGPU != null)
            {
                try
                {
                    //Load Texture for voxel grid
                    m_tex2DBorder = Game.Content.Load<Texture2D>("Border");

                    //Load Voxel Effect
                    var effect = Game.Content.Load<Effect>("CubeBrushShader");

                    effect.CurrentTechnique = effect.Techniques["Technique1"];
                    effect.Parameters["xDirectionalLightDir"].SetValue(LightDir);

                    //Assign Effect to GPU Mesh
                    m_genMesh.m_meshGPU.SingleEffect = effect;

                }
                catch (Exception _e)
                {
                    Debug.WriteLine(_e.Message);
                }

            }
        }

        void InitFromNewArray()
        {
            //Center World Matrix
            int middleX = (m_array.CUBESIZEX >> 1);
            int middleY = (m_array.CUBESIZEY >> 1);
            int middleZ = (m_array.CUBESIZEZ >> 1);

            m_vec3CubeOrigin.X = middleX;
            m_vec3CubeOrigin.Y = middleY;
            m_vec3CubeOrigin.Z = middleZ;

            //int offset = middle;
            m_vec3Origin = new Vector3(-middleX, -middleY, -middleZ); //Default center
            m_vec3Origin *= CubeHelpers.CUBE_SIZE;

            m_worldMtx = Matrix.Identity;
            m_worldMtx.Translation = m_vec3Origin;
        }


        #endregion

        #region -- Cubes Access --

        public void GetCube(int _x, int _y, int _z, out SmallCube _c)
        {
            m_array.GetCube(_x, _y, _z, out _c);
        }

        public void SetCube(int _x, int _y, int _z, SmallCube _c)
        {
            if (_x < 0 || _y < 0 || _z < 0) return;
            if (_x >= m_array.CUBESIZEX || _y >= m_array.CUBESIZEY || _z >= m_array.CUBESIZEZ) return;

            m_array.SetCube(_x, _y, _z, _c);
        }

        public bool RayCast(Vector3 _o, Vector3 _ray, float _dist, ref BoundingBox _obb, ref Vector3Int _oposcubeTrack, out SmallCube _ocube)
        {
            float cs = (float)CubeHelpers.CUBE_SIZE;

            //Step from _o to _ray
            float d = 0.0f;

            Vector3 p = _o;

            _ocube = CubeArray3D.m_emptyCube;

            //Debug.WriteLine("RayCast start at {0} dir {1}", p, _ray);

            while (d < _dist)
            {
                //Get cube at _o
                int cx = (int)(p.X / CubeHelpers.CUBE_SIZE); //World To Cube unit
                int cy = (int)(p.Y / CubeHelpers.CUBE_SIZE);
                int cz = (int)(p.Z / CubeHelpers.CUBE_SIZE);

                if (cx >= 0 && cy >= 0 && cz >= 0
                  && cx < m_array.CUBESIZEX && cy < m_array.CUBESIZEY && cz < m_array.CUBESIZEZ)
                {
                    SmallCube cube;
                    m_array.GetCube(cx, cy, cz, out cube);

                    if (cube.byMatL0 != 0)
                    {
                        //Debug.WriteLine("Cube Found at {0},{1},{2}", cx, cy, cz);

                        _oposcubeTrack.X = cx;//In cube unit
                        _oposcubeTrack.Y = cy;//In cube unit
                        _oposcubeTrack.Z = cz;

                        //BB
                        _obb.Min.X = (float)(cx * CubeHelpers.CUBE_SIZE); //To world coords
                        _obb.Min.Y = (float)(cy * CubeHelpers.CUBE_SIZE); //To world coords
                        _obb.Min.Z = (float)(cz * CubeHelpers.CUBE_SIZE); //To world coords

                        _obb.Max.X = _obb.Min.X + cs;
                        _obb.Max.Y = _obb.Min.Y + cs;
                        _obb.Max.Z = _obb.Min.Z + cs;

                        _ocube = cube;

                        return true;
                    }
                }

                d += 1.0f;
                p += _ray;
            }
            //Debug.WriteLine("RayCast end at {0}", p);


            return false;
        }

        /// <summary>
        /// Optimized ray cube intersection for shadow
        /// </summary>
        /// <param name="_o">start position in world coords</param>
        /// <param name="_dir"></param>
        /// <param name="_maxRayLen"></param>
        /// <param name="_from"></param>
        /// <returns></returns>
        public bool RayCastShadow(Vector3 _o, Vector3 _dir, float _maxRayLen, out float _ofFoundDist)
        {
            //Step from _o 
            float d = 0.0f;
            _ofFoundDist = 0.0f;

            Vector3 p = _o;   //In world unit

            while (d < _maxRayLen)
            {

                _ofFoundDist = d;

                //Get cube at p (p is in world unit)
                int cx = (int)(p.X / CubeHelpers.CUBE_SIZE); //To voxel unit
                int cy = (int)(p.Y / CubeHelpers.CUBE_SIZE);
                int cz = (int)(p.Z / CubeHelpers.CUBE_SIZE);

                if (cx >= 0 && cy >= 0 && cz >= 0
                  && cx < m_array.CUBESIZEX && cy < m_array.CUBESIZEY && cz < m_array.CUBESIZEZ)
                {
                    SmallCube cube;
                    m_array.GetCube(cx, cy, cz, out cube);

                    if (cube.byMatL0 != 0)
                    {
                        return true;
                    }
                }

                d += 1.0f;
                p += _dir;
            }

            return false;
        }

        #endregion

        #region -- Drawing for edition --
        public void Draw(Matrix view, Matrix proj)
        {
            if (m_genMesh.m_meshGPU == null)
                return;

            MeshGPU meshGPU = m_genMesh.m_meshGPU;

            if (meshGPU.m_npartsCount < 0)
                return;

            var effect = meshGPU.SingleEffect;

            effect.CurrentTechnique = effect.Techniques["Technique1"];
            effect.Parameters["xWorld"].SetValue(m_worldMtx);
            effect.Parameters["xView"].SetValue(view);
            effect.Parameters["xProjection"].SetValue(proj);


            effect.Parameters["xDirectionalLightDir"].SetValue(LightDir);

            if (m_tex2DBorder != null)
                effect.Parameters["xTexSlot0"].SetValue(m_tex2DBorder);

            if (ShowGridOnVoxel == true)
                effect.Parameters["xGridPower"].SetValue(1.0f);
            else
                effect.Parameters["xGridPower"].SetValue(0.0f);

            // Create a vertex buffer, and copy our vertex data into it.  
            GraphicsDevice device = effect.GraphicsDevice;

            device.RasterizerState = RasterizerState.CullCounterClockwise;// CullClockwise;
            device.DepthStencilState = DepthStencilState.Default;
            device.BlendState = BlendState.Opaque;

            effect.CurrentTechnique.Passes[0].Apply();  //One pass

            //Draw Grouped by Effect Slot
            for (int i = 0; i < meshGPU.m_npartsCount; i++)
            {
                MeshGPUPart part = meshGPU.m_meshParts[i];

                device.SetVertexBuffer(part.m_gpuVertexBuffer);
                device.Indices = part.m_gpuIndexBuffer;

                device.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, part.m_gpuIndexBuffer.IndexCount / 3);
            }


        }
        #endregion

        #region -- VBO --
        public GenMesh GenerateMesh()
        {

            //VBO
            m_genMesh.Clear();  //Set Dirty too

            int x, y, z;
            for (x = 0; x < m_array.CHUNKSIZEX; x++)
            {
                for (y = 0; y < m_array.CHUNKSIZEY; y++)
                {
                    for (z = 0; z < m_array.CHUNKSIZEZ; z++)
                    {
                        CubeChunk3D chunk = m_array.GetChunk(x, y, z);
                        if (chunk != null)
                        {
                            // PASS 1=> precalcul des occlusions par voxel
                            GenOcclusionForChunk(chunk, x << CubeArray3D.CHUNKSIZE_OPSHIFT, y << CubeArray3D.CHUNKSIZE_OPSHIFT, z << CubeArray3D.CHUNKSIZE_OPSHIFT);

                            // PASS 2=> Calcul des VBO
                            GenChunkVBO(chunk, x << CubeArray3D.CHUNKSIZE_OPSHIFT, y << CubeArray3D.CHUNKSIZE_OPSHIFT, z << CubeArray3D.CHUNKSIZE_OPSHIFT);
                        }
                    }
                }
            }

            m_genMesh.CreateGPUMesh(Game.GraphicsDevice, false);


            return m_genMesh;
        }




        //Generate cubes for a chunk
        //.position in chunk idx
        void GenChunkVBO(CubeChunk3D _chunk, int _chX, int _chY, int _chZ)
        {


            SmallCube c;
            int x, y, z;
            for (x = 0; x < CubeArray3D.CHUNKSIZE; x++)
            {
                for (y = 0; y < CubeArray3D.CHUNKSIZE; y++)
                {
                    for (z = 0; z < CubeArray3D.CHUNKSIZE; z++)
                    {
                        _chunk.GetCube(x, y, z, out c);
                        if (c.byMatL0 != 0)
                            GenCubeVBO2(c, _chX + x, _chY + y, _chZ + z, x, y, z);
                    }
                }
            }


        }

        //Generate a cube VBO
        //.position in CubeArray3D
        void GenCubeVBO2(SmallCube _c, int _cx, int _cy, int _cz, int _x, int _y, int _z)
        {
            int i, j;

            ////////////////////////// COMPUTE VISIBLES FACES MASK
            byte byVFMask = CalcVisibleFaces(_c, _cx, _cy, _cz);
            if (byVFMask == 0)
                return;

            ////////////////////////// COMPUTE VERTICES POS
            int nNumVertex;

            for (j = 0; j < 8; j++)
            {
                m_bVertexComputed[j] = false;
                m_vertComputed[j] = Vector3.Zero;
            }

            int wcx = _cx; //To World coord
            int wcy = _cy; //To World coord
            int wcz = _cz; //To World Coord


            //For each Cube's faces
            for (i = 0; i < 6; i++) //6 faces
            {
                //If face visibility precalc mask
                if ((byVFMask & (1 << i)) != 0)
                    for (j = 0; j < 4; j++) //4 corners
                    {
                        nNumVertex = CubeHelpers.fv[i, j];  //Face,Corner

                        if (m_bVertexComputed[nNumVertex] == false)  //if never calc
                        {
                            CubeHelpers.GetSolidWorldVertex(wcx, wcy, wcz, nNumVertex, out m_vertComputed[nNumVertex]);

                            m_vertComputed[nNumVertex] *= CubeHelpers.CUBE_SIZE;       //Scale cube size
                            m_bVertexComputed[nNumVertex] = true;
                        }
                    }
            }

            ////////////////////////// COMPUTE SHADOW LIGHTING
            GenCubeShadowLightAO(_c, _cx, _cy, _cz, _x, _y, _z, byVFMask, ref m_vertCols);

            ////////////////////////// COMPUTE CUBE FACES

            int v0, v1, v2, v3;
            int iv0, iv1, iv2, iv3;

            for (i = 0; i < 6; i++) //6 faces
            {
                if ((byVFMask & (1 << i)) == 0)
                    continue; //Face invisible


                v0 = CubeHelpers.fv[i, 0];  //Face,Corner
                v1 = CubeHelpers.fv[i, 1];  //Face,Corner
                v2 = CubeHelpers.fv[i, 2];  //Face,Corner
                v3 = CubeHelpers.fv[i, 3];  //Face,Corner


                int dim = i >> 1;  // 0,0,1,1,2,2 X,Y,Z

                j = i << 2; //Color index in m_vertCols

                /////////////////////
                //Get face normal
                Vector3 n1 = CubeHelpers.FacesPerSideNormal[i];

                ////////////////////////////////
                //Add vertices

                //V0
                iv0 = m_genMesh.AddVertex(ref m_vertComputed[v0], ref m_vertCols[j], ref n1);

                //V1
                iv1 = m_genMesh.AddVertex(ref m_vertComputed[v1], ref m_vertCols[j + 1], ref n1);

                //V2
                iv2 = m_genMesh.AddVertex(ref m_vertComputed[v2], ref m_vertCols[j + 2], ref n1);

                //V3
                iv3 = m_genMesh.AddVertex(ref m_vertComputed[v3], ref m_vertCols[j + 3], ref n1);

                ////////////////////////////////
                //Add Face

                m_genMesh.AddQuad((ushort)iv0, (ushort)iv1, (ushort)iv2, (ushort)iv3);


            }

        }

        public byte CalcVisibleFaces(SmallCube c, int cx, int cy, int cz)
        {
            byte byVFMask = 0x00;  //By default no faces visibles

            if (c.byMatL0 == 0)
                return byVFMask;

            //For each Cube's faces
            SmallCube nc;
            int i;

            for (i = 0; i < 6; i++) //6 faces
            {
                bool bvis = true;

                //check if neighbour face is solid
                m_array.GetNeighbourCube(cx, cy, cz, (CubeHelpers.SIDE)i, out nc);

                if (nc.byMatL0 != 0)
                    bvis = false;

                //Update visible face
                if (bvis)
                {
                    byVFMask |= (byte)(1 << i);
                }

            }

            return byVFMask;
        }

        #endregion

        #region -- AO 3DArray Generating --

        /// <summary>
        /// Done while BuildVBO generating
        /// </summary>
        void GenOcclusionForChunk(CubeChunk3D _chunk, int _chX, int _chY, int _chZ)
        {
            int x, y, z;

            //Reset occlusion map
            Array.Clear(m_abyOcclusionCubeMap, 0, m_abyOcclusionCubeMap.Length);

            //Make occlusion
            SmallCube c;
            for (x = 0; x < CubeArray3D.CHUNKSIZE; x++)
            {
                for (y = 0; y < CubeArray3D.CHUNKSIZE; y++)
                {
                    for (z = 0; z < CubeArray3D.CHUNKSIZE; z++)
                    {
                        _chunk.GetCube(x, y, z, out c);

                        //Opaque
                        if (c.byMatL0 != 0)
                        {
                            _GenOcclusionPerCube(_chX + x, _chY + y, _chZ + z, x, y, z);
                        }
                    }
                }
            }

        }

        /// <summary>
        ///
        /// </summary>
        /// _cx pos in zone (cube unit)
        /// _x pos in chunk (cube unit)
        void _GenOcclusionPerCube(int _cx, int _cy, int _cz, int _x, int _y, int _z)
        {
            Vector3 pos = new Vector3(_cx, _cy, _cz);  //Cube pos into world (in cube unit)
            pos *= CubeHelpers.CUBE_SIZE; //=> to world unit

            float fDelta = CubeHelpers.CUBE_SIZE;
            Vector3 vec3Dir = new Vector3(-1, fDelta, 0); //Light Dir

            float maxRayLength = Math.Max(Math.Max(m_array.CUBESIZEX, m_array.CUBESIZEY), m_array.CUBESIZEZ);

            float ofFoundDist;

            int aox = _x + 1;  //For array border
            int aoy = _y + 1;  //For array border
            int aoz = _z + 1;  //For array border

            //RayCast around voxel
            Vector3 posO;
            int x, y, z;
            for (x = -1; x <= 1; x++)
            {
                posO.X = pos.X + (x * CubeHelpers.CUBE_SIZE);

                for (z = -1; z <= 1; z++)
                {
                    int offset = ((aox + x) * (FlattenOffsetAO)) + ((aoz + z) * AO_CUBESIZE_Y) + aoy;

                    posO.Z = pos.Z + (z * CubeHelpers.CUBE_SIZE);

                    for (y = -1; y <= 1; y++)
                    {
                        if (m_abyOcclusionCubeMap[offset + y] != 0)
                            continue;

                        posO.Y = pos.Y + (y * CubeHelpers.CUBE_SIZE);

                        bool bOccluded = RayCastShadow(posO, vec3Dir, maxRayLength, out ofFoundDist);
                        if (bOccluded == true)
                            m_abyOcclusionCubeMap[offset + y] = 2;  //Parsed and Occluded
                        else
                            m_abyOcclusionCubeMap[offset + y] = 1; //Just parsed

                    }

                }

            }

        }


        #endregion

        #region -- AO Lighting --

        /// <summary>
        /// Done while BuildVBO generating (position in AOArray)
        /// </summary>
        void GenCubeShadowLightAO(SmallCube _cube, int _cx, int _cy, int _cz, int _aox, int _aoy, int _aoz, byte _byVFMask, ref Color[] _colors)
        {
            int i, j;

            Color gradCol = m_colPalette[_cube.byMatL0 - 1];

            //Lighting to face
            for (i = 0; i < 6; i++) //6 faces
            {
                if ((_byVFMask & (1 << i)) == 0)
                    continue; //Face invisible

                for (j = 0; j < 4; j++)
                {
                    int nNumVertex = CubeHelpers.fv[i, j];  //Face,Corner

                    byte byOcclusion = SmoothOcclusion(_cube, _aox, _aoy, _aoz, nNumVertex, i);

                    m_shadowVerts[j] = Color.Lerp(gradCol, Color.Black, (float)byOcclusion / 6.0f);
                }


                int k = i << 2;//*4

                _colors[k] = m_shadowVerts[0];
                _colors[k + 1] = m_shadowVerts[1];
                _colors[k + 2] = m_shadowVerts[2];
                _colors[k + 3] = m_shadowVerts[3];

            }

        }

        /// <summary>
        /// Smooth occlusion per face (position in AOArray)
        /// </summary>
        byte SmoothOcclusion(SmallCube _cube, int _aox, int _aoy, int _aoz, int _vidx, int _side)
        {
            byte oc = 0;
            int offset;

            Vector3Int o = new Vector3Int(_aox, _aoy, _aoz);

            Vector3Int r = CubeHelpers.SolidVertex[_vidx];


            int sign = _side & 1;
            int dim = _side >> 1;

            if (sign != 0)
                o[CubeHelpers.D[dim]] += 1;
            else
                o[CubeHelpers.D[dim]] -= 1;


            //current
            int x, y, z;
            int xx, yy, zz;
            x = o[0]; y = o[1]; z = o[2];

            x = x + 1;
            y = y + 1;
            z = z + 1;

            xx = x; yy = y; zz = z;

            offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy; //3darray[(x * (sy*sz)) + (z * sy) + y]
            offset = Math.Max(0, offset);
            offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
            oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);

            if (dim == 0)//X
            {
                int s1 = 1;
                int s2 = 1;
                if (r[1] == 0) s1 = -s1;  //corner
                if (r[2] == 0) s2 = -s2;  //corner

                //Y-Z
                yy = y; zz = z + s2;
                offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy;
                offset = Math.Max(0, offset);
                offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
                oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);

                yy = y + s1; zz = z;
                offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy;
                offset = Math.Max(0, offset);
                offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
                oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);

                yy = y + s1; zz = z + s2;
                offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy;
                offset = Math.Max(0, offset);
                offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
                oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);

            }
            else if (dim == 1)//Y
            {
                int s1 = 1;
                int s2 = 1;
                if (r[0] == 0) s1 = -s1;  //corner
                if (r[2] == 0) s2 = -s2;  //corner

                //X-Z
                xx = x; zz = z + s2;
                offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy;
                offset = Math.Max(0, offset);
                offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
                oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);

                xx = x + s1; zz = z;
                offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy;
                offset = Math.Max(0, offset);
                offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
                oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);

                xx = x + s1; zz = z + s2;
                offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy;
                offset = Math.Max(0, offset);
                offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
                oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);


            }
            else if (dim == 2) //Z
            {
                int s1 = 1;
                int s2 = 1;
                if (r[0] == 0) s1 = -s1;  //corner
                if (r[1] == 0) s2 = -s2;  //corner

                //X-Y
                xx = x; yy = y + s2;
                offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy;
                offset = Math.Max(0, offset);
                offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
                oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);

                xx = x + s1; yy = y;
                offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy;
                offset = Math.Max(0, offset);
                offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
                oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);

                xx = x + s1; yy = y + s2;
                offset = (xx * (FlattenOffsetAO)) + (zz * AO_CUBESIZE_Y) + yy;
                offset = Math.Max(0, offset);
                offset = Math.Min(offset, (AO_CUBESIZE_X * AO_CUBESIZE_Y * AO_CUBESIZE_Z) - 1);
                oc += (byte)((int)m_abyOcclusionCubeMap[offset] >> 1);


            }


            return oc;
        }

        #endregion

        #region -- Helpers --
        void BuildDefaultColorPalette()
        {
            m_colPalette = new Color[256];

            //Build colors palette
            int idx = 0;
            float h, s, l;

            //Build greys
            for (l = 0; l < 1.0f; l += (1.0f / 16.0f))
            {
                m_colPalette[idx++] = ColorHelpers.HSLToColor(0, 0, l);
            }

            //Build colored
            s = 1f;
            for (l = 0; l < 1.0f; l += (1.0f / 16.0f))
            {
                for (h = 0; h < 360; h += (360 / 14))
                {
                    m_colPalette[idx++] = ColorHelpers.HSLToColor(h, s, (l * 0.90f) + 0.1f);
                }
                if (idx >= m_colPalette.Length) break;
            }

        }
        #endregion

    }

}
