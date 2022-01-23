using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class UIExecution : MonoBehaviour
{

	private static readonly Queue<Action> _Waiting_call = new Queue<Action>();
	private static UIExecution _instance = null;
		
	public void Update() {
		lock(_Waiting_call) {
			while (_Waiting_call.Count > 0) {
				_Waiting_call.Dequeue().Invoke();
			}
		}
	}

	public void Execute(IEnumerator action) {
		lock (_Waiting_call) {
			_Waiting_call.Enqueue (() => {
				StartCoroutine (action);
			});
		}
	}


	public void Execute(Action action)
	{
		Execute(CoroutineAction(action));
	}
	IEnumerator CoroutineAction(Action a)
	{
		a();
		yield return null;
	}


	public static bool Exists() {
		return _instance != null;
	}

	public static UIExecution Instance() {
		if (!Exists ()) {
			throw new Exception ("UI execution instance not found");
		}
		return _instance;
	}


	void Awake() {

		_instance = this as UIExecution;
		
	}
}
