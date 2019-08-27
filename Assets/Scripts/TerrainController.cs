using UnityEngine;

public class TerrainController : MonoBehaviour {

	public Terrain terrain;
	public int strength;

	int heightMapWidth;
	int heightMapHeight;
	float[,] heights;
	TerrainData terrainData;

	
	void Start () 
	{
		terrainData = terrain.terrainData;
		heightMapWidth = terrainData.heightmapWidth;
		heightMapHeight = terrainData.heightmapHeight;
		heights = terrainData.GetHeights(0, 0, heightMapWidth, heightMapHeight);
	}
	
	public void lowerTerrain (Vector3 point) 
	{
		// gets positions on terrain.
		int mouseX = (int)((point.x / terrainData.size.x) * heightMapWidth);
		int mouseY = (int)((point.z / terrainData.size.z) * heightMapHeight);

		// get height at point
		var modifiedHeights = new float[1,1];
		float y = heights[mouseX, mouseY];
		y -= strength;

		if (y < terrainData.size.y)
		{
			y = terrainData.size.y;
		}

		modifiedHeights[0,0] = y;
		heights[mouseX, mouseY] = y;
		terrainData.SetHeights(mouseX, mouseY, modifiedHeights);
	}
}
