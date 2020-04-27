using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AccelerometerClicker : MonoBehaviour
{
	private List<float> accelerations;

	//public int seconds = 2;
	public Player player;
	private GvrBasePointer pointer;
	private int lag = 15;
	private float threshold = 10f;

	// Start is called before the first frame update
	void Start()
	{
		accelerations = new List<float>();
		pointer = GetComponent<GvrBasePointer>();
	}

	// Update is called once per frame
	void Update()
	{
		var newAccelerations = Input.accelerationEvents.ToList();
		var skip = Math.Min(accelerations.Count + newAccelerations.Count - lag * 5, 0);
		
		accelerations = accelerations
			.Skip(skip)
			//.Where(item => item.deltaTime > since)
			.Union(newAccelerations
				.Select(item => item.acceleration.magnitude)
			)
			.ToList();

		if (HasPeak(accelerations))
		{
			accelerations = new List<float>();
			var obj = pointer.CurrentRaycastResult.gameObject;
			player.SelectItem(obj);
		}
	}
	
	private bool HasPeak(IReadOnlyList<float> input)
	{
		if (input.Count * 2 >= lag)
		{
			for (int i = lag; i < input.Count; i++)
			{
				var slidingWindow = input.Skip(i - lag).Take(lag).ToList();

				var average = slidingWindow.Average();
				var std = StdDev(slidingWindow, average);

				var value = Math.Abs(input[i] - average);
				var test = threshold * std;
				if (value > test)
				{
					return true;
				}
			}
		}
		else
		{
			Debug.Log(input.Count);
		}

		return false;
	}

	private float StdDev(IReadOnlyList<float> values, float average)
	{
		float sum = (float) values.Sum(d => Math.Pow(d - average, 2));
		return (float) Math.Sqrt(sum / (values.Count - 1));
	}
}