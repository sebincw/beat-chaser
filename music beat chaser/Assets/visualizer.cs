using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class visualizer : MonoBehaviour
{
	public AudioSource[] audio_sources;
	public AudioSource audio_1;
	public float[] spectrum_data;
	public GameObject cube;
	public GameObject[] cubes;
	public int frame_count;
	public float[] total_v_avg;
	public int[] freqs;

	private Rigidbody rb;

	void Start()
	{
		rb = GetComponent<Rigidbody>();
		audio_sources = GetComponents<AudioSource>();
		audio_1 = audio_sources[0];
		audio_1.Play();
		spectrum_data = new float[1024];
		cubes = new GameObject[10];
		total_v_avg = new float[20];
		freqs = new int[] {0,800,1000};

		for (int i = 0; i < freqs.Length; i++)
		{
			cubes[i] = Instantiate(cube,new Vector3(2*i,0,0) ,Quaternion.identity);
		}
	}


	void LateUpdate()
	{
		GameObject.Find("Main Camera").transform.position = transform.position + new Vector3(0,30,-30);
		GameObject.Find("Main Camera").transform.LookAt(transform);
	}

	void Update()
	{
		rb.velocity = new Vector3(0,0,10);

		if (frame_count >= 200)
		{
			frame_count = 1;
		}


		audio_1.GetSpectrumData(spectrum_data, 0, FFTWindow.Hanning);



		for (int i = 0; i < spectrum_data.Length; i++)
		{
			Debug.DrawLine(new Vector3(i, 0, 0), new Vector3(i, spectrum_data[i] * 1000, 0), Color.red);
		}

		frame_count++;

		for (int i = 0; i < freqs.Length - 1; i++)
		{
			if (isbeat(freqs[i], freqs[i + 1], i))
			{
				Instantiate(cube, new Vector3(100 * i, 0, transform.position.z), Quaternion.identity);
				//cubes[i].transform.localScale = new Vector3(1,100,1);
			}
		}




	}


	bool isbeat(int p,int q, int v_index)
	{
		float v_avg;

		v_avg = 0;

		for (int i = p; i < q; i++)
		{
			v_avg += spectrum_data[i];
		}
		v_avg /= (q-p);
		total_v_avg[v_index] = ((total_v_avg[v_index] * (frame_count - 1)) + v_avg) / frame_count;

		if (v_avg > 1.7f * total_v_avg[v_index])
			return true;
		else
			return false;
	}
}
