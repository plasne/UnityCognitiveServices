using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LUIS : MonoBehaviour
{
	public string query { get; set; }
	public TopScoringIntent topScoringIntent { get; set; }
	public List<Intent> intents { get; set; }
	public List<Entity> entities { get; set; }

}

[Serializable]
public class TopScoringIntent
{
	public string intent { get; set; }
	public double score { get; set; }
}

[Serializable]
public class Intent
{
	public string intent { get; set; }
	public double score { get; set; }
}

[Serializable]
public class Entity
{
	public string entity { get; set; }
	public string type { get; set; }
	public int startiIndex { get; set; }
	public int endIndex { get; set; }
	public double score { get; set; }
}
