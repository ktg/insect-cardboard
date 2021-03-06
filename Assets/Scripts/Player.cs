﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
	public enum Condition
	{
		TimeElapsed,
		InsectsCaught
	}

	private enum State
	{
		Tutorial,
		Game,
		GameOver
	}

	public Transform holdPoint;
	public GameObject tutorialRoot;

	[Header("Game End")] public Condition condition = Condition.TimeElapsed;
	public float conditionTimeElapsed = 120f;
	public int conditionInsectsCaught = 10;

	[Header("UI")] public GameObject uiRoot;
	public Text scoreText;
	public Text correctText;
	public Text incorrectText;

	private GameObject heldInsect;

	private State state = State.Tutorial;
	private float gameUntil;

	private readonly List<GameObject> insects = new List<GameObject>();

	private int score;
	private int correct;
	private int incorrect;

	// Start is called before the first frame update
	void Start()
	{
		foreach (var insect in GameObject.FindGameObjectsWithTag("Wasp"))
		{
			insects.Add(insect);
		}

		foreach (var insect in GameObject.FindGameObjectsWithTag("Hoverfly"))
		{
			insects.Add(insect);
		}

		StartTutorial();
	}

	// Update is called once per frame
	void Update()
	{
		if (state == State.Game)
		{
			switch (condition)
			{
				case Condition.InsectsCaught:
				{
					if (correct + incorrect >= conditionInsectsCaught)
					{
						EndGame();
					}

					break;
				}
				case Condition.TimeElapsed:
				{
					if (Time.time > gameUntil)
					{
						EndGame();
					}

					break;
				}
			}
		}

		if (Input.GetMouseButtonDown(0))
		{
			Debug.Log(EventSystem.current.currentSelectedGameObject);
		}

		if (heldInsect != null)
		{
			var heldTransform = heldInsect.transform;
			heldTransform.position = holdPoint.position;
			heldTransform.rotation = holdPoint.rotation;
		}
	}

	private void StartGame()
	{
		state = State.Game;
		foreach (var insect in insects)
		{
			insect.SetActive(true);
		}

		uiRoot.SetActive(false);
		tutorialRoot.SetActive(false);
	}

	private void EndGame()
	{
		heldInsect = null;
		state = State.GameOver;
		uiRoot.SetActive(true);
		scoreText.text = score.ToString();
		correctText.text = correct.ToString();
		incorrectText.text = incorrect.ToString();
		foreach (var insect in insects)
		{
			insect.SetActive(false);
		}
	}

	public void StartTutorial()
	{
		Debug.Log("Start Tutorial");
		foreach (var insect in insects)
		{
			insect.SetActive(false);
		}

		state = State.Tutorial;
		uiRoot.SetActive(false);
		tutorialRoot.SetActive(true);
		score = 0;
		correct = 0;
		incorrect = 0;
		var jars = FindObjectsOfType<JarScript>();
		foreach (var jar in jars)
		{
			jar.ResetScore();
		}

		gameUntil = Time.time + conditionTimeElapsed;
		//text.SetText("");
	}

	private void ReleaseInsect()
	{
		if (heldInsect == null) return;
		heldInsect.GetComponent<DrunkenWasp>().OnHeldEnd();
		heldInsect = null;
	}

	public void SelectItem(GameObject item)
	{
		Debug.Log(item);
		if (item != null)
		{
			var waspScript = item.GetComponent<DrunkenWasp>();
			if (waspScript != null)
			{
				ReleaseInsect();
				waspScript.OnHeldStart();
				heldInsect = item;
				return;
			}

			var jarScript = item.GetComponent<JarScript>();
			if (jarScript != null)
			{
				if (heldInsect == null) return;
				if (state == State.Tutorial)
				{
					StartGame();
				}
				else
				{
					if (jarScript.Add(heldInsect))
					{
						correct++;
						score++;
					}
					else
					{
						incorrect++;
						score--;
					}
				}
			}
		}

		ReleaseInsect();
	}
}