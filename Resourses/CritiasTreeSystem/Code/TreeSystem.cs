// Copyright Ioan-Bogdan Lazu. All Rights Reserved.

using System;
using System.Reflection;
using System.Collections.Generic;

using UnityEngine;

[System.Serializable]
public struct TreeSystemStoredInstance
{
    public int m_TreeHash;
    public Matrix4x4 m_PositionMtx;
    public Vector3 m_WorldPosition;
    public Vector3 m_WorldScale;
    public float m_WorldRotation;
    public Bounds m_WorldBounds;
}

[System.Serializable]
public class TreeSystemStructuredTrees
{
    public Bounds m_Bounds;
    public RowCol m_Position;

    public TreeSystemStoredInstance[] m_Instances;
    // TODO: hold extra LOD data to see LOD level, fade level etc...
    // TODO: maybe instantiate this data only if the cell is visible in order to save memory
    public TreeSystemLODInstance[] m_InstanceData;
}

[System.Serializable]
public class TreeSystemLODData
{
    public bool m_IsBillboard;

    public float m_StartDistance;
    public float m_EndDistance;

    public Material[] m_Materials;
    public Mesh m_Mesh;

    // BEGIN RUNTIME UPDATED DATA
    public Renderer m_TreeRenderer;
    public MaterialPropertyBlock m_Block;
    // END RUNTIME UPDATED DATA

    public void CopyBlock()
    {
        // If we are a 3D model, copy the wind data
        if (m_IsBillboard == false)
            m_TreeRenderer.GetPropertyBlock(m_Block);
    }

    public bool IsInRange(float distance)
    {
        if (distance < m_StartDistance || distance > m_EndDistance)
            return false;

        return true;
    }
}

[System.Serializable]
public class TreeSystemPrototypeData
{
    // BEGIN DATA MANUALLY SET
    public TextAsset m_TreeBillboardData;
    // END DATA MANUALLY SET

    // BEGIN DATA GENERATED
    public GameObject m_TreePrototype;
    // Not using index any more, but hash so that we are sure that it is not lost upon any reorder
    public int m_TreePrototypeHash;
    public Vector3 m_Size; // Width, height, bottom. To be used by the tree system for each instance
    public Material m_BillboardBatchMaterial; // Billboard material to be used for tree billboards
    public Material m_BillboardMasterMaterial;
    public Vector2[] m_VertBillboardUVs;
    public Vector2[] m_HorzBillboardUVs;

    // Maximum LOD level including billboard as index
    public int m_MaxLodIndex;
    // Maximum LOD level excluding billboard as index
    public int m_MaxLod3DIndex;
    // END DATA GENERATED

    // BEGIN UPDATED AT RUNTIME
    public TreeSystemLODData[] m_LODData;
    // END UPDATED AT RUNTIME
}

/**
 * This guy is going to be stored at the cell's tree index, so that we don't need to hold that data internally
 */
[System.Serializable]
public struct TreeSystemLODInstance
{
    public int m_LODLevel;
    public float m_LODTransition;
    public float m_LODFullFade;
}

// public class TreeSystem
[System.Serializable]
public class TreeSystemTerrain
{
    public Terrain m_ManagedTerrain;    
    public Bounds m_ManagedTerrainBounds;

    public int m_CellSize;
    public int m_CellCount;

    // If we need to get the closes 1 neighbor, or 2 neighbors etc...
    public int m_NeighborRetrievalDepth = 1;

    // Trees for that terrain
    public TreeSystemStructuredTrees[] m_Cells;

    // TODO: not used at the moment, modify after
    // Build at runtime, for very quick neighbor retrieval
    public TreeSystemStructuredTrees[,] m_StructuredCells;
}

[System.Serializable]
public class TreeSystemSettings
{
    public float m_MaxTreeDistance = 300;

    // 5 meters when we're fading between LOD levels
    public float m_LODTranzitionThreshold = 5.0f;
    public float m_LODFadeSpeed = 5.0f;
}

public class TreeSystem : MonoBehaviour
{
    private static readonly int MAX_BATCH = 1000;

    // Data to add
    private TreeSystemSettings m_Settings = new TreeSystemSettings();

    public Shader m_ShaderTreeMaster;
    public Shader m_ShaderBillboardMaster;

    public int m_ShaderIDFadeLOD;    
    public int m_ShaderIDBillboardScaleRotation;

    public static void SetMaterialBillProps(TreeSystemPrototypeData data, Material m)
    {
        Vector2[] uv = data.m_VertBillboardUVs;

        Vector4[] UV_U = new Vector4[8];
        Vector4[] UV_V = new Vector4[8];

        for (int i = 0; i < 8; i++)
        {
            // 4 by 4 elements
            UV_U[i].x = uv[4 * i + 0].x;
            UV_U[i].y = uv[4 * i + 1].x;
            UV_U[i].z = uv[4 * i + 2].x;
            UV_U[i].w = uv[4 * i + 3].x;

            UV_V[i].x = uv[4 * i + 0].y;
            UV_V[i].y = uv[4 * i + 1].y;
            UV_V[i].z = uv[4 * i + 2].y;
            UV_V[i].w = uv[4 * i + 3].y;
        }

        m.SetVectorArray("_UVVert_U", UV_U);
        m.SetVectorArray("_UVVert_V", UV_V);

        uv = data.m_HorzBillboardUVs;

        for (int i = 0; i < 1; i++)
        {
            // 4 by 4 elements
            UV_U[i].x = uv[4 * i + 0].x;
            UV_U[i].y = uv[4 * i + 1].x;
            UV_U[i].z = uv[4 * i + 2].x;
            UV_U[i].w = uv[4 * i + 3].x;

            UV_V[i].x = uv[4 * i + 0].y;
            UV_V[i].y = uv[4 * i + 1].y;
            UV_V[i].z = uv[4 * i + 2].y;
            UV_V[i].w = uv[4 * i + 3].y;
        }

        m.SetVector("_UVHorz_U", UV_U[0]);
        m.SetVector("_UVHorz_V", UV_V[0]);        
    }

    private void UpdateLODDataDistances(TreeSystemPrototypeData data)
    {
        LOD[] lods = data.m_TreePrototype.GetComponent<LODGroup>().GetLODs();
        TreeSystemLODData[] lodData = data.m_LODData;

        for (int i = 0; i < lodData.Length; i++)
        {
            // If it's the billboard move on
            if (i == lods.Length - 1)
                continue;

            TreeSystemLODData d = lodData[i];

            if (i == 0)
            {
                // If it's first 3D LOD
                d.m_StartDistance = 0;
                d.m_EndDistance = ((1.0f - lods[i].screenRelativeTransitionHeight) * m_Settings.m_MaxTreeDistance);
            }
            else if (i == lodData.Length - 2)
            {
                // If it's last 3D LOD
                d.m_StartDistance = ((1.0f - lods[i - 1].screenRelativeTransitionHeight) * m_Settings.m_MaxTreeDistance);
                d.m_EndDistance = m_Settings.m_MaxTreeDistance;
            }
            else
            {
                // If it's a LOD in between
                d.m_StartDistance = ((1.0f - lods[i - 1].screenRelativeTransitionHeight) * m_Settings.m_MaxTreeDistance);
                d.m_EndDistance = ((1.0f - lods[i].screenRelativeTransitionHeight) * m_Settings.m_MaxTreeDistance);
            }
        }        
    }

    private void GenerateRuntimePrototypeData(TreeSystemPrototypeData data)
    {
        // All stuff that we draw must have billboard renderers, that's why it's trees
        LOD[] lods = data.m_TreePrototype.GetComponent<LODGroup>().GetLODs();
        TreeSystemLODData[] lodData = data.m_LODData;        

        // Set material's UV at runtime
        SetMaterialBillProps(data, data.m_BillboardBatchMaterial);
        SetMaterialBillProps(data, data.m_BillboardMasterMaterial);

        // Get the same tree stuff as the grass
        for (int i = 0; i < lodData.Length; i++)
        {
            if (lods[i].renderers.Length != 1)
                Debug.LogError("Renderer length != 1! Problem!");

            // Assume only one renderer            
            TreeSystemLODData d = lodData[i];
            
            // If it's last element
            if (i == lods.Length - 1)
            {
                d.m_Block = new MaterialPropertyBlock();                
            }
            else
            {
                d.m_Block = new MaterialPropertyBlock();

                GameObject weirdTree = Instantiate(lods[i].renderers[0].gameObject);
                weirdTree.hideFlags = HideFlags.HideAndDontSave;
                
                // Get the tree renderer
                d.m_TreeRenderer = weirdTree.GetComponent<MeshRenderer>();
                d.m_TreeRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

                d.m_TreeRenderer.transform.SetParent(Camera.main.transform);
                d.m_TreeRenderer.transform.localPosition = new Vector3(0, 0, -2);
                d.m_TreeRenderer.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

                MeshFilter mf = d.m_TreeRenderer.GetComponent<MeshFilter>();

                // Get an instance mesh from the weird tree
                Mesh m = mf.mesh;

                // Set the new bounds so that it's always drawn
                Bounds b = m.bounds;
                b.Expand(50.0f);
                m.bounds = b;
                mf.mesh = m;
            }
        }
        
        // Update the LOD distances
        UpdateLODDataDistances(data);
    }
    
    public static TreeSystem Instance;
    
    public TreeSystemPrototypeData[] m_ManagedPrototypes;        
    private Dictionary<int, TreeSystemPrototypeData> m_ManagedPrototypesIndexed;
    
    // Managed terrain systems
    public TreeSystemTerrain[] m_ManagedTerrains;       
    
    public void SetTreeDistance(float distance)
    {
        if (distance != m_Settings.m_MaxTreeDistance)
        {
            Shader.SetGlobalFloat("_TreeSystemDistance", distance);

            m_Settings.m_MaxTreeDistance = distance;

            for (int i = 0; i < m_ManagedPrototypes.Length; i++)
                UpdateLODDataDistances(m_ManagedPrototypes[i]);
        }
    }
    
    public float GetTreeDistance()
    {
        return m_Settings.m_MaxTreeDistance;
    }    

    public float GetTreeTanzitionThreshold()
    {
        return m_Settings.m_LODTranzitionThreshold;
    }

    void Awake()
    {        
        Instance = this;

        // Get the non-alloc version of the plane extraction
        MethodInfo info = typeof(GeometryUtility).GetMethod("Internal_ExtractPlanes", BindingFlags.Static | BindingFlags.NonPublic, null, new Type[] { typeof(Plane[]), typeof(Matrix4x4) }, null);
        ExtractPlanes = Delegate.CreateDelegate(typeof(Action<Plane[], Matrix4x4>), info) as Action<Plane[], Matrix4x4>;

        // Set maximum tree distance
        Shader.SetGlobalFloat("_TreeSystemDistance", m_Settings.m_MaxTreeDistance);

        m_ShaderIDFadeLOD = Shader.PropertyToID("master_LODFade");
        m_ShaderIDBillboardScaleRotation = Shader.PropertyToID("_InstanceScaleRotation");

        // Generate runtime data
        for (int i = 0; i < m_ManagedPrototypes.Length; i++)
            GenerateRuntimePrototypeData(m_ManagedPrototypes[i]);
                
        // Build the dictionary based on the index
        m_ManagedPrototypesIndexed = new Dictionary<int, TreeSystemPrototypeData>();
        for (int i = 0; i < m_ManagedPrototypes.Length; i++)
            m_ManagedPrototypesIndexed.Add(m_ManagedPrototypes[i].m_TreePrototypeHash, m_ManagedPrototypes[i]);                        
    }
    
    private int[] m_IdxTemp = new int[MAX_BATCH];
    private float[] m_DstTemp = new float[MAX_BATCH];
    
    private Action<Plane[], Matrix4x4> ExtractPlanes;
    private Plane[] m_PlanesTemp = new Plane[6];
    private Vector3 m_CameraPosTemp;

    void Start()
    {
        for (int i = 0; i < m_ManagedTerrains.Length; i++)
        {
            TreeSystemTerrain terrain = m_ManagedTerrains[i];
            terrain.m_ManagedTerrain.drawTreesAndFoliage = false;

            for (int j = 0; j < terrain.m_Cells.Length; j++)
            {
                // TODO: maybe allocate dinamically at runtime based on cell visibility to save memory
                terrain.m_Cells[j].m_InstanceData = new TreeSystemLODInstance[terrain.m_Cells[j].m_Instances.Length];

                // TODO: structure cell data
            }
        }
    }

    private Vector3[] m_TempCorners = new Vector3[8];

    private int m_DataIssuedMeshTrees = 0;
    private int m_DataIssuedTerrainCells = 0;
    private int m_DataIssuedTerrains = 0;
    private int m_DataIssuesDrawCalls = 0;

    void Update()
    {        
        m_DataIssuedMeshTrees = 0;
        m_DataIssuedTerrainCells = 0;
        m_DataIssuedTerrains = 0;
        m_DataIssuesDrawCalls = 0;

        Camera camera = Camera.main;

        // Calculate planes and camera position
        ExtractPlanes(m_PlanesTemp, camera.projectionMatrix * camera.worldToCameraMatrix);
        m_CameraPosTemp = camera.transform.position;

        // Update the wind block data
        for (int proto = 0; proto < m_ManagedPrototypes.Length; proto++)
        {
            TreeSystemLODData[] lod = m_ManagedPrototypes[proto].m_LODData;
            for (int i = 0; i < lod.Length; i++)
                lod[i].CopyBlock();
        }

        float x, y, z;
        float treeDistSqr = m_Settings.m_MaxTreeDistance * m_Settings.m_MaxTreeDistance;        

        for (int i = 0; i < m_ManagedTerrains.Length; i++)
        {
            TreeSystemTerrain terrain = m_ManagedTerrains[i];

            // Get closest point
            Vector3 pt = terrain.m_ManagedTerrainBounds.ClosestPoint(m_CameraPosTemp);

            // Check if terrain is within reach range
            x = pt.x - m_CameraPosTemp.x;
            y = pt.y - m_CameraPosTemp.y;
            z = pt.z - m_CameraPosTemp.z;

            float distToTerrain = x * x + y * y + z * z;

            if(distToTerrain < treeDistSqr)
            {
                // If the terrain is within tree range
                ProcessTerrain(terrain, ref treeDistSqr);
                m_DataIssuedTerrains++;
            }
        }

        if(Time.frameCount % 60 == 0)
        {
            Debug.Log("Issued terrains: " + m_DataIssuedTerrains + " issued cells: " + m_DataIssuedTerrainCells + " issued trees:" + m_DataIssuedMeshTrees + " issued draw calls:" + m_DataIssuesDrawCalls);
        }
    }

    private void ProcessTerrain(TreeSystemTerrain terrain, ref float treeDistSqr)
    {                        
        TreeSystemStructuredTrees[] cells = terrain.m_Cells;

        float x, y, z;

        // TODO: calculate based on bounds index the cells that we'll iterate like the grass system
        // We won't require to iterate all the cells but the neighbors of the current cell only

        // Go bounds by bounds
        for (int cellIdx = 0; cellIdx < cells.Length; cellIdx++)
        {
            TreeSystemStructuredTrees cell = cells[cellIdx];

            // If we don't have any tree skip this cell
            if (cell.m_Instances.Length <= 0) continue;

            // Get closest point to cell
            Vector3 pt = cell.m_Bounds.ClosestPoint(m_CameraPosTemp);

            x = pt.x - m_CameraPosTemp.x;
            y = pt.y - m_CameraPosTemp.y;
            z = pt.z - m_CameraPosTemp.z;

            float distToCell = x * x + y * y + z * z;

            if (distToCell < treeDistSqr && GeometryUtility.TestPlanesAABB(m_PlanesTemp, cell.m_Bounds))
            {                 
                // TODO: the same process when we are going to have terrain sub-cells
                               
                ProcessTerrainCell(cell, ref treeDistSqr);

                // If it's visible
                m_DataIssuedTerrainCells++;
            }            
        }
    }

    private void ProcessTerrainCell(TreeSystemStructuredTrees cell, ref float treeDistSqr)
    {
        // Draw all trees instanced in MAX_BATCH chunks
        int tempIndex = 0;
        float x, y, z;

        // If we are completely inside frustum we don't need to AABB test each tree
        bool insideFrustum = TUtils.IsCompletelyInsideFrustum(m_PlanesTemp, TUtils.BoundsCorners(ref cell.m_Bounds, ref m_TempCorners));

        // Tree instances
        TreeSystemStoredInstance[] treeInstances = cell.m_Instances;
        
        // TODO: Hm... if we take that hash it doesn't mean that it's the first visible one...
        int treeHash = treeInstances[0].m_TreeHash;
        int currentTreeHash = treeHash;
        
        for (int treeIndex = 0; treeIndex < treeInstances.Length; treeIndex++)
        {
            // 1.33 ms for 110k trees
            // This is an order of magnitude faster than (treeInstances[treeIndex].m_WorldPosition - pos).sqrMagnitude
            // since it does not initiate with a ctor an extra vector during the computation
            x = treeInstances[treeIndex].m_WorldPosition.x - m_CameraPosTemp.x;
            y = treeInstances[treeIndex].m_WorldPosition.y - m_CameraPosTemp.y;
            z = treeInstances[treeIndex].m_WorldPosition.z - m_CameraPosTemp.z;

            float distToTree = x * x + y * y + z * z;

            // 17 ms for 110k trees
            // float distToTree = (treeInstances[treeIndex].m_WorldPosition - pos).sqrMagnitude;

            if (insideFrustum)
            {
                // If we are completely inside the frustum we don't need to check each individual tree's bounds
                if (distToTree <= treeDistSqr)
                {
                    currentTreeHash = treeInstances[treeIndex].m_TreeHash;

                    if (tempIndex >= MAX_BATCH || treeHash != currentTreeHash)
                    {
                        IssueDrawTrees(m_ManagedPrototypesIndexed[treeHash], cell, m_IdxTemp, m_DstTemp, tempIndex);
                        tempIndex = 0;

                        // Update the hash
                        treeHash = currentTreeHash;
                    }

                    m_IdxTemp[tempIndex] = treeIndex;
                    m_DstTemp[tempIndex] = Mathf.Sqrt(distToTree);
                    tempIndex++;

                    m_DataIssuedMeshTrees++;
                }
            }
            else
            {
                // If we are not completely inside the frustum we need to check the bounds of each individual tree
                if (distToTree <= treeDistSqr && GeometryUtility.TestPlanesAABB(m_PlanesTemp, treeInstances[treeIndex].m_WorldBounds))
                {
                    currentTreeHash = treeInstances[treeIndex].m_TreeHash;

                    if (tempIndex >= MAX_BATCH || treeHash != currentTreeHash)
                    {
                        IssueDrawTrees(m_ManagedPrototypesIndexed[treeHash], cell, m_IdxTemp, m_DstTemp, tempIndex);
                        tempIndex = 0;

                        // Update the hash
                        treeHash = currentTreeHash;
                    }

                    m_IdxTemp[tempIndex] = treeIndex;
                    m_DstTemp[tempIndex] = Mathf.Sqrt(distToTree);
                    tempIndex++;

                    m_DataIssuedMeshTrees++;
                }
            }
        } // End cell tree iteration

        if (tempIndex > 0)
        {
            // Get a tree hash from the first element of the array so that we know for sure that we use the correc prototype data
            IssueDrawTrees(m_ManagedPrototypesIndexed[treeInstances[m_IdxTemp[0]].m_TreeHash], cell,
                m_IdxTemp, m_DstTemp, tempIndex);

            tempIndex = 0;
        }
    }

    public bool m_UseInstancing = true;

    private static readonly int MAX_LOD_COUNT = 5;

    // 'MAX_LOD_COUNT' is the maximum possible count of LOD levels. Modify based on the tree with the highest LOD value.
    private Matrix4x4[] m_MtxLODTemp_0 = new Matrix4x4[MAX_BATCH];
    private Matrix4x4[] m_MtxLODTemp_1 = new Matrix4x4[MAX_BATCH];
    private Matrix4x4[] m_MtxLODTemp_2 = new Matrix4x4[MAX_BATCH];
    private Matrix4x4[] m_MtxLODTemp_3 = new Matrix4x4[MAX_BATCH];
    private Matrix4x4[] m_MtxLODTemp_4 = new Matrix4x4[MAX_BATCH];

    // How many of that certain lod level we have
    private int[] m_MtxLODTempCount = new int[MAX_LOD_COUNT];

    private void IssueDrawTrees(TreeSystemPrototypeData data, TreeSystemStructuredTrees trees, int[] indices, float[] dist, int count)
    {
        for(int i = 0; i < MAX_LOD_COUNT; i++)
            m_MtxLODTempCount[i] = 0;        

        TreeSystemLODData[] lodData = data.m_LODData;

        TreeSystemStoredInstance[] treeInstances = trees.m_Instances;
        TreeSystemLODInstance[] lodInstanceData = trees.m_InstanceData;

        int maxLod3D = data.m_MaxLod3DIndex;

        // Process LOD so that we know what to batch
        for(int i = 0; i < count; i++)
        {
            ProcessLOD(ref lodData, ref maxLod3D, ref lodInstanceData[indices[i]], ref dist[i]);
        }

        if (m_UseInstancing)
        {
            // Build the batched stuff. We want to batch the same LOD and draw them using instancing
            for (int i = 0; i < count; i++)
            {
                int idx = indices[i];
                int currentLODLevel = lodInstanceData[idx].m_LODLevel;

                // Collect that LOD level
                if (currentLODLevel == 0)
                    m_MtxLODTemp_0[m_MtxLODTempCount[currentLODLevel]] = treeInstances[idx].m_PositionMtx;
                else if(currentLODLevel == 1)
                    m_MtxLODTemp_1[m_MtxLODTempCount[currentLODLevel]] = treeInstances[idx].m_PositionMtx;
                else if (currentLODLevel == 2)
                    m_MtxLODTemp_2[m_MtxLODTempCount[currentLODLevel]] = treeInstances[idx].m_PositionMtx;
                else if (currentLODLevel == 3)
                    m_MtxLODTemp_3[m_MtxLODTempCount[currentLODLevel]] = treeInstances[idx].m_PositionMtx;
                else if (currentLODLevel == 4)
                    m_MtxLODTemp_4[m_MtxLODTempCount[currentLODLevel]] = treeInstances[idx].m_PositionMtx;

                m_MtxLODTempCount[currentLODLevel]++;

                // We don't instantiate the trees that are in a transition since they should be an exception
                if (idx == maxLod3D && lodInstanceData[idx].m_LODFullFade < 1)
                    DrawBillboardLOD(ref lodData[currentLODLevel + 1], ref lodInstanceData[idx], ref treeInstances[idx]);
            }
            
            // Now that the data is built, issue it
            for (int i = 0; i <= maxLod3D; i++)
            {
                if(i == 0)
                    Draw3DLODInstanced(ref lodData[i], ref m_MtxLODTemp_0, ref m_MtxLODTempCount[i]);
                else if(i == 1)
                    Draw3DLODInstanced(ref lodData[i], ref m_MtxLODTemp_1, ref m_MtxLODTempCount[i]);
                else if (i == 2)
                    Draw3DLODInstanced(ref lodData[i], ref m_MtxLODTemp_2, ref m_MtxLODTempCount[i]);
                else if (i == 3)
                    Draw3DLODInstanced(ref lodData[i], ref m_MtxLODTemp_3, ref m_MtxLODTempCount[i]);
                else if (i == 4)
                    Draw3DLODInstanced(ref lodData[i], ref m_MtxLODTemp_4, ref m_MtxLODTempCount[i]);
            }
        }
        else
        {
            // Draw the processed lod
            for (int i = 0; i < count; i++)
            {
                int idx = indices[i];
                DrawProcessedLOD(ref lodData, ref maxLod3D, ref lodInstanceData[idx], ref treeInstances[idx]);
            }
        }
    }
    
    // Draw and process routines
    private void DrawProcessedLOD(ref TreeSystemLODData[] data, ref int maxLod3D, ref TreeSystemLODInstance lodInst, ref TreeSystemStoredInstance inst)
    {
        int lod = lodInst.m_LODLevel;

        // Draw the stuff with the material specific for each LOD
        Draw3DLOD(ref data[lod], ref lodInst, ref inst);

        if (lod == maxLod3D && lodInst.m_LODFullFade < 1)
        {
            // Since we only need for the last 3D lod the calculations...
            DrawBillboardLOD(ref data[lod + 1], ref lodInst, ref inst);
        }
    }
           
    /**
     * Must be called only if the camera distance is smaller than the maximum tree view distance.
     */
    private void ProcessLOD(ref TreeSystemLODData[] data, ref int max3DLOD, ref TreeSystemLODInstance inst, ref float cameraDistance)
    {
        if (cameraDistance < m_Settings.m_MaxTreeDistance - m_Settings.m_LODTranzitionThreshold)
        {
            for (int i = 0; i < data.Length - 1; i++)
            {
                if (data[i].IsInRange(cameraDistance))
                {
                    // Calculate lod tranzition value
                    if (cameraDistance > data[i].m_EndDistance - m_Settings.m_LODTranzitionThreshold)
                        inst.m_LODTransition = 1.0f - (data[i].m_EndDistance - cameraDistance) / m_Settings.m_LODTranzitionThreshold;
                    else
                        inst.m_LODTransition = 0.0f;

                    inst.m_LODLevel = i;
                    break;
                }
            }
            
            if (inst.m_LODLevel == max3DLOD && inst.m_LODFullFade < 1)
            {
                // If we are the last 3D lod then we animate nicely
                inst.m_LODFullFade += Time.deltaTime * m_Settings.m_LODFadeSpeed;
            }
            else if(inst.m_LODFullFade < 1)
            {
                // If we are not the lost LOD level simply set the value to 1
                inst.m_LODFullFade = 1f;
            }
        }
        else
        {
            inst.m_LODLevel = max3DLOD;
            if (inst.m_LODFullFade > 0) inst.m_LODFullFade -= Time.deltaTime * m_Settings.m_LODFadeSpeed;
        }
    }
    
    public void Draw3DLODInstanced(ref TreeSystemLODData data, ref Matrix4x4[] positions, ref int count)
    {
        if (count > 0)
        {
            // TODO: set the array stuff
            data.m_Block.SetVector(m_ShaderIDFadeLOD, new Vector4(1, 1, 0, 0));

            for (int mat = 0; mat < data.m_Materials.Length; mat++)
            {
                Graphics.DrawMeshInstanced(data.m_Mesh, mat, data.m_Materials[mat], positions, count, data.m_Block, 
                    UnityEngine.Rendering.ShadowCastingMode.On, true);

                m_DataIssuesDrawCalls++;
            }
        }
    }

    public void Draw3DLOD(ref TreeSystemLODData data, ref TreeSystemLODInstance lodInst, ref TreeSystemStoredInstance inst)
    {
        data.m_Block.SetVector(m_ShaderIDFadeLOD, new Vector4(lodInst.m_LODTransition, lodInst.m_LODFullFade, 0, 0));
        
        for (int mat = 0; mat < data.m_Materials.Length; mat++)
        {
            Graphics.DrawMesh(data.m_Mesh, inst.m_PositionMtx, data.m_Materials[mat], 0, null, mat, data.m_Block, true, true);
            m_DataIssuesDrawCalls++;
        }
    }

    public void DrawBillboardLOD(ref TreeSystemLODData data, ref TreeSystemLODInstance lodInst, ref TreeSystemStoredInstance inst)
    {
        data.m_Block.SetVector(m_ShaderIDFadeLOD, new Vector4(lodInst.m_LODTransition, 1.0f - lodInst.m_LODFullFade, 0, 0));

        // Set extra scale and stuff
        Vector4 extra = inst.m_WorldScale;
        extra.w = inst.m_WorldRotation;
        data.m_Block.SetVector(m_ShaderIDBillboardScaleRotation, extra);
        
        for (int mat = 0; mat < data.m_Materials.Length; mat++)
        {            
            Graphics.DrawMesh(data.m_Mesh, inst.m_PositionMtx, data.m_Materials[mat], 0, null, mat, data.m_Block, true, true);
            m_DataIssuesDrawCalls++;
        }
    }       
}
