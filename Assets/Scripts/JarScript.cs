using UnityEngine;

public class JarScript : MonoBehaviour
{
	public string tag = "Hoverfly";
	private int score;
	private GameObject[] pointPool;

	// Start is called before the first frame update
	void Start()
	{
		pointPool = GameObject.FindGameObjectsWithTag("Score");
		ResetScore();
	}

	// Update is called once per frame
	void Update()
	{
		for (var i = 0; i < pointPool.Length; i++)
		{
			var insect = pointPool[i];
			insect.SetActive(i < score);
		}
	}

	public void ResetScore()
	{
		score = 0;
	}

	public bool Add(GameObject insect)
	{
		if (insect.CompareTag(tag))
		{
			score++;
			return true;
		}
		else
		{
			score--;
			return false;
		}
	}
}