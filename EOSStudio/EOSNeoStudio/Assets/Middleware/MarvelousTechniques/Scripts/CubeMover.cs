//----------------------------------------------
//            Marvelous Techniques
// Copyright © 2015 - Arto Vaarala, Kirnu Interactive
// http://www.kirnuarp.com
//----------------------------------------------

using System;
using UnityEngine;
using System.Collections;
using System.Threading;



//[ExecuteInEditMode]
public class CubeMover : MonoBehaviour {

	public float xSpeed = 0f;
	public float ySpeed = 0f;
	public float zSpeed = 0f;
	public float maxMovement = 5f;
	Vector3 startPos = Vector3.zero;
	bool forward = true;
	// Use this for initialization
	void Start () {
		startPos = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 p = transform.position;

		p.x += xSpeed * Time.deltaTime*(forward?1:-1);
		p.y += ySpeed * Time.deltaTime*(forward?1:-1);
		p.z += zSpeed * Time.deltaTime*(forward?1:-1);

		transform.position = p;
		if (Vector3.Distance (startPos, p) >= maxMovement) {
			forward = !forward;
			startPos = transform.position;
		}
	}
}


// public class CubeMover : MonoBehaviour
// {
//     void Start()
//     {
//         Thread thread = new Thread(new ThreadStart(LoadTest));
//         thread.IsBackground = true;
//         thread.Start();        
//     }
//
//     private void Update()
//     {
//         Debug.Log("Update  `~~~~~~");
//     }
//
//     static void LoadTest()
//     {
//         for (int i = 0; i < 1000; i++)
//         {
//             Debug.Log($"load : {i}");
//             Thread.Sleep(100);
//         }
//     }
// }


// using System.Threading.Tasks;
// using System.Threading;
// using UnityEngine;
//
// public class CubeMover : MonoBehaviour
// {
//     void Start()
//     {
//         Debug.Log($"Run() invoked in Start() {Time.realtimeSinceStartup}");
//         Run(10);
//         Debug.Log($"Run() returns {Time.realtimeSinceStartup}");
//     }
//
//     void Update()
//     {
//         Debug.Log($"Update() {Time.realtimeSinceStartup}");
//     }
//
//     async void Run(int count)
//     {
//         int result = 0;
//
//         await Task.Run(() =>
//         {
//             for (int i = 0; i < count; ++i)
//             {
//                 Debug.Log($"reproces :${i}");
//                 result += i;
//                 Debug.Log($"reproces ###########:${i}");
//                 Thread.Sleep(1000);
//             }
//         });
//
//         Debug.Log($" Test Result : {Time.realtimeSinceStartup}" + result);
//     }
//     async void Run2(int count)
//     {
//         int result = 0;
//
//         await Task.Run(() =>
//         {
//             for (int i = 0; i < count; ++i)
//             {
//                 Debug.Log($"reproces2 :${i}");
//                 result += i;
//                 Thread.Sleep(1000);
//             }
//         });
//
//         Debug.Log($" run2 Result : {Time.realtimeSinceStartup}" + result);
//     }
// }