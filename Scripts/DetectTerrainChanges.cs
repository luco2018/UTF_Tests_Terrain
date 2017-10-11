using UnityEngine;

[ExecuteInEditMode]
public class DetectTerrainChanges : MonoBehaviour
{
	void OnTerrainChanged(TerrainChangedFlags flags)
	{
		if ((flags & TerrainChangedFlags.Heightmap) != 0)
		{
			Debug.Log("Heightmap changes");
		}

		if ((flags & TerrainChangedFlags.DelayedHeightmapUpdate) != 0)
		{
			Debug.Log("Heightmap painting");
		}

		if ((flags & TerrainChangedFlags.TreeInstances) != 0)
		{
			Debug.Log("Tree changes");
		}
	}
}