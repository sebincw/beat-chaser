using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubescript : MonoBehaviour
{


	void Start()
	{
		Destroy(gameObject, 3);
	}


	void FixedUpdate()
	{
		//if(transform.localScale.y > 1)
		//transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y - 0.6f, transform.localScale.z);
	}
}
