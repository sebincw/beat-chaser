using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class playerauto : MonoBehaviour
{

	public float[][] spectrum_out;

	public bool test, left, right, sliderflag, toleft, toright, repeatflag;
	public GameObject lrtcube, slidercube, lrtbcube, floor;
	public float prevz, vel, seek, lr, s1, s2, prevtp, prevtime, spectrum_out_size, smpls, v_sum,avg_v;
	public AudioSource[] asources;
	public AudioSource a1, a2, a3, a4;
	public int tpindex, speedmul, score, k, j;
	public string path, displaydata;
	public List<float> timingpoints;
	public int[] cubelr;
	public Vector3 currpos;
	public Text t1;
	public float[] spec;

	public float smoothTime = 0.3F;
	public Vector3 velocity = Vector3.zero;

	void Start()
	{
		spec = new float[128];

		asources = GetComponents<AudioSource>();
		a1 = asources[0];
		a2 = asources[1];
		a3 = asources[2];

		smpls = a1.clip.samples;

		spectrum_out = get_spectrum_data();
		spectrum_out_size = spectrum_out.Length;

		Screen.SetResolution(Screen.width / 2, Screen.height / 2, true);


		cubelr = new int[] { -1, 1 };
		lr = transform.position.x;
		vel = 3;
		speedmul = 20;
		prevtp = 0;

		for (int i = 0; i < timingpoints.Count; i++)
		{

			if (timingpoints[i] - prevtp > 4)
				Instantiate(lrtcube, new Vector3(vel, 2, timingpoints[i]), Quaternion.identity);
			else
				Instantiate(lrtbcube, new Vector3(vel, 2, timingpoints[i]), Quaternion.identity);



			if (vel == 15)
				vel -= 6;
			else if (vel == -15)
				vel += 6;
			else
				vel += 6 * cubelr[UnityEngine.Random.Range(0, 2)];


			prevtp = timingpoints[i];
		}

		a1.Play();
		prevtime = Time.time;

		
	}



	void LateUpdate()
	{
		GameObject.Find("Main Camera").transform.position = new Vector3(0, 65, transform.position.z - 10);
	}


	void Update()
	{
		//t1.text = score.ToString();

		a1.GetSpectrumData(spec, 1, FFTWindow.Rectangular);

		v_sum = 0;
		avg_v = 0;
		for (int k = 0; k < 128; k++)
		{
			v_sum += spec[k];
		}
		avg_v = v_sum / 128;




		Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, (seek * speedmul) + (speedmul / 3.18f));
		transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);


		seek = a1.time;

		s1 = seek * speedmul;
		s2 = transform.position.z;




		if (((Input.GetKey(KeyCode.D)) && (Input.GetKey(KeyCode.A))) || (left && right))
		{
			//transform.position = new Vector3(transform.position.x, 5, seek * speedmul);
		}
		else if ((Input.GetKeyDown(KeyCode.A)) || (left))
		{
			transform.Translate(Vector3.left * 6);
			left = false;
		}
		else if ((Input.GetKeyDown(KeyCode.D)) || (right))
		{
			transform.Translate(Vector3.left * -6);
			right = false;
		}









		if (transform.position.z > prevz - 100)
		{
			prevz += 100;
			Instantiate(floor, new Vector3(0, 0, prevz), Quaternion.identity);
		}



		if (Input.touchCount == 1)
		{
			if (Input.GetTouch(0).phase.Equals(TouchPhase.Began))
			{
				{
					if (Input.GetTouch(0).position.x < Screen.width / 2)
						left = true;
					else
						left = false;
					if (Input.GetTouch(0).position.x > Screen.width / 2)
						right = true;
					else
						right = false;
				}
			}
		}
	}





	void OnTriggerEnter(Collider col)
	{

		if (col.gameObject.tag == "death")
		{
			a3.Play();
			score--;
		}

		if (col.gameObject.tag == "point")
		{
			a2.Play();
			Destroy(col.gameObject);
			score++;
		}

		if (col.gameObject.tag == "delete")
		{
			Destroy(col.gameObject);
		}

	}


	float[][] get_spectrum_data()
	{
		float volume_sum = 0, avg_volume = 0, song_time_multiplier;
		int samples = a1.clip.samples;
		int sample_rate = a1.clip.frequency / 100;
		int arr_siz = samples / sample_rate;
		FFTWindow win = FFTWindow.Rectangular;


		song_time_multiplier = a1.clip.length / arr_siz;


		float[][] spectrum = new float[arr_siz][];
		for (int i = 0; i < arr_siz; i++)
		{
			spectrum[i] = new float[128];
		}

		for (int i = 1, j = 0; j < arr_siz; i += sample_rate, j++)
		{
			a1.Play();
			a1.timeSamples = i;
			a1.GetSpectrumData(spectrum[j], 0, win);

			volume_sum = 0;
			avg_volume = 0;
			for (int k = 0; k < 128; k++)
			{
				volume_sum += spectrum[j][k];
			}
			avg_volume = volume_sum / 128;
			if (avg_volume > 0.01f)
			{
				timingpoints.Add(j * song_time_multiplier);
			}

		}

		return spectrum;
	}
}
