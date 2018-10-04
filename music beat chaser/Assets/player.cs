using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using UnityEngine.Audio;

public class player : MonoBehaviour
{

	public bool test,left,right,sliderflag,toleft,toright,repeatflag,decoyplaying;
	public GameObject lrtcube, slidercube, lrtbcube, floor, plight, undercube;
	public float prevz, vel, avgvol, volsum, maxvol, seek, sliderpoint, movetimer, s1,s2,prevtime, slidermultiplier, msperbeat, songvel, prevtp, maxvoli, delaytimer;
	public float frame_count;
	public float[] spectrum, songvels, songveltimes, total_v_avg, freq_muls;
	public AudioSource[] asources;
	public AudioSource a1, a2, a3, a4;
	public int tpindex,speedmul,score,k, j;
	public string path,displaydata;
	public List <float> timingpoints,sliderlength,repeats,repeatpoints;
	public int[] cubelr;
	public Vector3 currpos;
	public Text t1;
	public TextAsset ft;
	public AudioMixer masterMixer;
	public int[] freqs;


	public float smoothTime = 0.3F;
	public Vector3 velocity = Vector3.zero;

	void Start()
	{
		slidermultiplier = 0.8999f;
		msperbeat = 342.857142857143f;

		songvels = new float[16];
		songveltimes = new float[16];
		songvel = 100f;

		songvels[0] = 100;
		songvels[1] = 100;

		songveltimes[0] = 45.358f;
		songveltimes[1] = 56.330f;


		Screen.SetResolution(Screen.width/2, Screen.height/2, true);


		freq_muls = new float[] { 1.6f, 1.8f, 1.5f };
		freqs = new int[] { 0, 50, 1000, 2048 };
		total_v_avg = new float[20];
		cubelr = new int[] {-1,1};
		spectrum = new float[2048];
		vel = 3;

		asources = GetComponents<AudioSource>();
		a1 = asources[0];
		a2 = asources[1];
		a3 = asources[2];
		a4 = asources[3];

		speedmul = 20;
		prevtp = 0;

		//load_fromfile();

		a1.Play();
		prevtime = Time.time;
		SetSound();
	}



	void LateUpdate()
	{
		GameObject.Find("Main Camera").transform.position = new Vector3(0, 65, transform.position.z - 10);
	}


	void Update()
	{

		frame_count++;
		if (frame_count >= 200)
		{
			frame_count = 1;
		}


		autodetect();


		t1.text = score.ToString();


		if (songveltimes[j] != 0)
		if (seek >= songveltimes[j])
		{
			songvel = songvels[j];
			j++;
		}



		Vector3 targetPosition = new Vector3(transform.position.x, transform.position.y, (seek * speedmul) + (speedmul/3.18f));
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

		//else if (Input.touchCount == 2)
		//{
		//	if (((Input.GetTouch(0).position.x < Screen.width / 2) && (Input.GetTouch(1).position.x > Screen.width / 2))|| ((Input.GetTouch(0).position.x > Screen.width / 2) && (Input.GetTouch(1).position.x < Screen.width / 2)))
		//	{
		//		left = true;
		//		right = true;
		//	}
		//	else
		//	{
		//		left = false;
		//		right = false;
		//	}
		//}
		//else
		//{
		//	left = false;
		//	right = false;
		//}


	}









	public void parseTextAsset(TextAsset ft)
	{

		string fs = ft.text;
		string[] fLines = Regex.Split(fs, "\n|\r|\r\n");

		for (int i = 0; i < fLines.Length; i++)
		{

			string valueLine = fLines[i];
			string[] values = Regex.Split(valueLine, ","); // your splitter here
			Myparse(values);

		}
	}


	public void Myparse(string[] myline)
	{
		if ((myline.Length > 1)&&(myline.Length < 7))
		{
			timingpoints.Add((float.Parse(myline[2]) / (1000 / speedmul)));
			sliderlength.Add(0);
			repeats.Add(1);
		}
		else if (myline.Length > 7)
		{
			timingpoints.Add((float.Parse(myline[2]) / (1000 / speedmul)));
			sliderlength.Add(((msperbeat * ((float.Parse(myline[7])) / (slidermultiplier * 100 * songvel))) / (1000 / speedmul)) * 100);
			repeats.Add(float.Parse(myline[6]));
		}
	}

	void OnTriggerEnter(Collider col)
	{

		if (col.gameObject.tag == "death")
		{
			a3.Play();
			score-= 5;
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



	public void autodetect()
	{
		volsum = 0;


		if (a1.time > 5.0f && !decoyplaying)
		{
			a4.Play();
			decoyplaying = true;
		}

		a1.GetSpectrumData(spectrum,0,FFTWindow.Hanning);

		for (int i = 0; i < spectrum.Length; i++)
		{
			Debug.DrawLine(new Vector3(i, 0, 0), new Vector3(i, spectrum[i] * 1000, 0), Color.red);
			volsum += spectrum[i];
			if (spectrum[i] > maxvol)
			{
				maxvol = spectrum[i];
				maxvoli = i;
			}
		}
		avgvol = volsum / spectrum.Length;

		//if ((spectrum[8] > 0.2f)&&(Time.time > delaytimer + 0.5f))
		//{
		//	if (vel == 15)
		//		vel -= 6;
		//	else if (vel == -15)
		//		vel += 6;
		//	else
		//		vel += 6 * cubelr[UnityEngine.Random.Range(0, 2)];

		//	//timingpoints.Add((a1.time + 4.8f) * speedmul);
		//	Instantiate(lrtcube, new Vector3(vel, 2, (a1.time + 4.95f) * speedmul), Quaternion.identity);
		//	delaytimer = Time.time;
		//}


		for (int i = 0; i < freqs.Length - 1; i++)
		{
			if (isbeat(freqs[i], freqs[i + 1], i))
			{
				if (Time.time > delaytimer + 0.4f)
				{
					if (vel == 15)
						vel -= 6;
					else if (vel == -15)
						vel += 6;
					else
						vel += 6 * cubelr[UnityEngine.Random.Range(0, 2)];

					delaytimer = Time.time;
					Instantiate(lrtcube, new Vector3(vel, 2, (a1.time + 4.95f) * speedmul), Quaternion.identity);
				}
				else if (Time.time > delaytimer + 0.2f)
				{
					if (vel == 15)
						vel -= 6;
					else if (vel == -15)
						vel += 6;
					else
						vel += 6 * cubelr[UnityEngine.Random.Range(0, 2)];

					delaytimer = Time.time;
					Instantiate(lrtbcube, new Vector3(vel, 2, (a1.time + 4.95f) * speedmul), Quaternion.identity);
				}
				//else if (Time.time > delaytimer + 0.1f)
				//{
				//	delaytimer = Time.time;
				//	Instantiate(lrtbcube, new Vector3(vel, 2, (a1.time + 4.95f) * speedmul), Quaternion.identity);
				//}
			}
		}
	}





	public void SetSound()
	{
		masterMixer.SetFloat("musicVol", -80);
	}


	bool isbeat(int p, int q, int v_index)
	{
		float v_avg;

		v_avg = 0;

		for (int i = p; i < q; i++)
		{
			v_avg += spectrum[i];
		}
		v_avg /= (q - p);
		total_v_avg[v_index] = ((total_v_avg[v_index] * (frame_count - 1)) + v_avg) / frame_count;

		if (v_avg > freq_muls[v_index] * total_v_avg[v_index])
			return true;
		else if ((v_index == 0) && (v_avg > 0.04f))
			return true;
		else
			return false;
	}


	public void load_fromfile()
	{
		ft = Resources.Load<TextAsset>("mayday");
		parseTextAsset(ft);

		for (int i = 0; i < timingpoints.Count; i++)
		{

			if (timingpoints[i] - prevtp > 4)
				Instantiate(lrtcube, new Vector3(vel, 2, timingpoints[i]), Quaternion.identity);
			else
				Instantiate(lrtbcube, new Vector3(vel, 2, timingpoints[i]), Quaternion.identity);

			if (sliderlength[i] > 0)
			{
				slidercube.transform.localScale = new Vector3(slidercube.transform.localScale.x, slidercube.transform.localScale.y, (sliderlength[i] * repeats[i]));
				Instantiate(slidercube, new Vector3(vel, 2, timingpoints[i] + ((sliderlength[i] * repeats[i]) / 2)), Quaternion.identity);

				for (int j = 1; j <= repeats[i]; j++)
					Instantiate(lrtbcube, new Vector3(vel, 2, timingpoints[i] + (sliderlength[i] * j)), Quaternion.identity);
			}


			if (vel == 15)
				vel -= 6;
			else if (vel == -15)
				vel += 6;
			else
				vel += 6 * cubelr[UnityEngine.Random.Range(0, 2)];
			prevtp = timingpoints[i];
		}
	}
}
