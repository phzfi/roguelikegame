using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LosResult
{
	public bool blocked = true;
	public List<Vector2i> openTiles;
	public List<Vector2i> blockedTiles;
}

public class LineOfSight : MonoBehaviour {
	
	// Checks line of sight between two grid coordinates. If getTileLists is true, lists describing blocked and open tiles along the line are returned as well.
	public static LosResult CheckLOS(NavPathAgent mover, Vector2i start, Vector2i stop, float range, bool getTileLists = false)
	{
		LosResult result = new LosResult();

		if(getTileLists)
		{
			result.blockedTiles = new List<Vector2i>();
			result.openTiles = new List<Vector2i>();
		}

		if(start == stop)
		{
			if (getTileLists)
				result.openTiles.Add(start);
			result.blocked = false;
			return result;
		}
		
		Vector2i dirI = stop - start;
		Vector2 dir = new Vector2(dirI.x, dirI.y);
		if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
			dir /= Mathf.Abs(dir.x);
		else
			dir /= Mathf.Abs(dir.y);
		
		Vector2 currentPos = new Vector2(start.x + .5f, start.y + .5f);
		Vector2i currentTile = start;
		bool LosBlocked = false;
		while(true)
		{
			if(!LosBlocked && mover.CanAccess(currentTile))
			{
				if (getTileLists && !result.openTiles.Contains(currentTile))
					result.openTiles.Add(currentTile);
			}
			else
			{
				LosBlocked = true;
				if (!getTileLists)
					break;
				else if (!result.blockedTiles.Contains(currentTile))
					result.blockedTiles.Add(currentTile);
			}

			currentPos += dir;
			currentTile = new Vector2i((int)currentPos.x, (int)currentPos.y);

			if (currentTile.Distance(start) > range)
				break;
		}

		return result;
	}
}
