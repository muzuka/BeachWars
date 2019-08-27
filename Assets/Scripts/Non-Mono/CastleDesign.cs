using UnityEngine;
using System.Collections.Generic;

public class CastleDesign {

	Dictionary<Vector3, bool> isBuiltRef;
	List<Vector3> blocks;

	CastleDesign() {
		isBuiltRef = new Dictionary<Vector3, bool>();
		blocks = new List<Vector3>();
	}

	CastleDesign(List<Vector3> list) {
		isBuiltRef = new Dictionary<Vector3, bool>();
		blocks = list;

		for(int i = 0; i < list.Count; i++) {
			isBuiltRef.Add(blocks[i], false);
		}
	}

	CastleDesign(Vector3[] list) {
		isBuiltRef = new Dictionary<Vector3, bool>();
		blocks = new List<Vector3>();

		for(int i = 0; i < list.Length; i++) {
			isBuiltRef.Add(list[i], false);
			blocks.Add(list[i]);
		}
	}


}
