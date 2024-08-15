using UnityEngine;

public class TerrainController : MonoBehaviour {

	public Terrain Terrain;
	public int Strength;

	int _heightMapWidth;
	int _heightMapHeight;
	float[,] _heights;
	TerrainData _terrainData;

	
	void Start() 
	{
		_terrainData = Terrain.terrainData;
		_heightMapWidth = _terrainData.heightmapResolution;
		_heightMapHeight = _terrainData.heightmapResolution;
		_heights = _terrainData.GetHeights(0, 0, _heightMapWidth, _heightMapHeight);
	}
	
	public void LowerTerrain(Vector3 point) 
	{
		// gets positions on terrain.
		int mouseX = (int)((point.x / _terrainData.size.x) * _heightMapWidth);
		int mouseY = (int)((point.z / _terrainData.size.z) * _heightMapHeight);

		// get height at point
		var modifiedHeights = new float[1,1];
		float y = _heights[mouseX, mouseY];
		y -= Strength;

		if (y < _terrainData.size.y)
		{
			y = _terrainData.size.y;
		}

		modifiedHeights[0,0] = y;
		_heights[mouseX, mouseY] = y;
		_terrainData.SetHeights(mouseX, mouseY, modifiedHeights);
	}
}
