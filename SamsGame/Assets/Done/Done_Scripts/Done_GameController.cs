using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;

[System.Serializable]	// makes it so the Unity Editor can see this type
public class MutantMachine : System.Object {
	// graphicTexture is the texture of the drawing Sam did of the ship in question
	public Texture graphicTexture;
	// range in feet it can shoot
	public int range;
	// health rating out of ten
	public int health;
	// strength rating out of ten
	public int strength;
	// name of the ship
	public string name;
	// cost of the ship in United States Dollars
	public int cost;
	// sound of the ship shooting
	public AudioClip shootSound;
}

[System.Serializable]	// makes it so the Unity Editor can see this type
public class ObjectThatCanHitYou : System.Object {
	// graphicTexture is the drawing Sam made of the object you can hit
	public Texture graphicTexture;
	// if damage < 0, then it subtracts that many points from the HEALTH of the player it hits
	// if damage > 0, then it adds that many points to HEALTH of player it hits
	public int damage;
	// if strength < 0, then it subtracts that many points from the STRENGTH of the player it hits
	// if strength > 0, then it adds that many points to the STRENGTH of the player it hits
	public int strength;
	// if money > 0, then it adds MONEY amount to the points of the player
	public int money;
	// the name of the object
	public string name;
}

public class Done_GameController : MonoBehaviour
{
	// array of objects that can hit you
	public ObjectThatCanHitYou[] objectsThatCanHitYou;
	// array of mutant machines
	public MutantMachine[] machines;
	// array of hazards of all types
	public GameObject[] hazards;
	// array of backgrounds for the game
	public Texture[] backgrounds;
	// the current background texture, which changes over time
	public GameObject curBackground;
	// the number of the background we are currently on in the array of backgrounds
	private int nBackground;
	// the dictionary which matches game objects to mutant machines and/or objects that can hit you
	private IDictionary<GameObject, string> hazardToDefinition = new Dictionary<GameObject, string>();
	// the prefab we use for 2d graphics assets
	public GameObject graphicsPrefab;

	public Vector3 spawnValues;
	public int hazardCount;
	public float spawnWait;
	public float startWait;
	public float waveWait;
	
	public GUIText scoreText;
	public GUIText restartText;
	public GUIText gameOverText;
	
	private bool gameOver;
	private bool restart;
	private int score;
	
	void Start ()
	{
		gameOver = false;
		restart = false;
		restartText.text = "";
		gameOverText.text = "";
		score = 0;
		UpdateScore ();
		StartCoroutine (SpawnWaves ());

		System.Timers.Timer aTimer = new System.Timers.Timer();
		aTimer.Elapsed += new ElapsedEventHandler(OnTimedEvent);
		// every 10 seconds
		aTimer.Interval = 10000;
		aTimer.Enabled = true;

		nBackground = 0;
		curBackground.GetComponent<MeshRenderer> ().material.mainTexture = backgrounds[nBackground];
	}

	private void OnTimedEvent(object source, ElapsedEventArgs e)
	{
		if (backgrounds.Length == 0)
			return;
		nBackground += 1;
		if (nBackground > backgrounds.Length)
			nBackground = 0;
		// now change the background
		curBackground.GetComponent<MeshRenderer> ().material.mainTexture = backgrounds[nBackground];
		return;
	}
	
	void Update ()
	{
		if (restart && Input.GetKeyDown (KeyCode.R))
			Application.LoadLevel (Application.loadedLevel);
	}

	void SetLayerRecursively(GameObject obj, int newLayer)
	{
		if (null == obj)
			return;

		obj.layer = newLayer;

		foreach (Transform child in obj.transform)
		{
			if (null == child)
				continue;
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}
	
	IEnumerator SpawnWaves ()
	{
		yield return new WaitForSeconds (startWait);
		while (true)
		{
			for (int i = 0; i < hazardCount; i++)
			{
				int rando = UnityEngine.Random.Range (0, hazards.Length);

				Vector3 spawnPosition = new Vector3 (UnityEngine.Random.Range (-spawnValues.x, spawnValues.x), spawnValues.y, spawnValues.z);
				Quaternion spawnRotation = Quaternion.identity;
				GameObject hazard = Instantiate (hazards [rando], spawnPosition, spawnRotation) as GameObject;

				// is it an enemy ship?
				if (rando == hazards.Length - 1) {
					// make the ship invisible
					hazard.layer = 4;
					SetLayerRecursively (hazard, 4);
					// first off, randomly select a mutant machine
					MutantMachine curMachine = machines [ UnityEngine.Random.Range (0, machines.Length)];
					// next, make the hazard have a picture of the mutant machine
					GameObject graphic = Instantiate (graphicsPrefab, spawnPosition, spawnRotation) as GameObject;
					graphic.transform.Rotate(0, 180, 0);
					graphic.GetComponent<MeshRenderer> ().material.mainTexture = curMachine.graphicTexture;
					graphic.transform.parent = hazard.transform;
					hazardToDefinition.Add (hazard, curMachine.name);

				} else {
					// assign a comet or other such random item to the picture
					hazard.layer = 5;
					SetLayerRecursively (hazard, 5);
					// randomly select a hazard of some sort (a thing you can hit)
					ObjectThatCanHitYou curObject = objectsThatCanHitYou [ UnityEngine.Random.Range(0, objectsThatCanHitYou.Length)];
					// next, make the hazard have a picture of the object that can hit you
					GameObject graphic = Instantiate (graphicsPrefab, spawnPosition, spawnRotation) as GameObject;
					graphic.transform.Rotate (0, 180, 0);
					graphic.GetComponent<MeshRenderer> ().material.mainTexture = curObject.graphicTexture;
					graphic.transform.parent = hazard.transform;
					hazardToDefinition.Add (hazard, curObject.name);

				}
				yield return new WaitForSeconds (spawnWait);
			}
			yield return new WaitForSeconds (waveWait);
			
			if (gameOver)
			{
				restartText.text = "Press 'R' for Restart";
				restart = true;
				break;
			}
		}
	}
	
	public void AddScore (int newScoreValue)
	{
		score += newScoreValue;
		UpdateScore ();
	}
	
	void UpdateScore ()
	{
		scoreText.text = "Score: " + score;
	}
	
	public void GameOver ()
	{
		gameOverText.text = "Game Over!";
		gameOver = true;
	}
}