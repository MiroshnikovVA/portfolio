using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpaceShooter {
	class ObjectPool<T> where T : Component {

		public List<T> ActiveObjects = new List<T>();
		List<T> _poolObjects = new List<T>();
		T[] _prefabs;
		Transform _thisTransform;
		GameObject _projectilesPoolParent;

		public void Destroy() {
			GameObject.Destroy(_projectilesPoolParent);
		}

		public ObjectPool(Transform thisTransform, params T[] prefabs) {
			_thisTransform = thisTransform;
			_projectilesPoolParent = new GameObject("<ObjectPool>");
			_prefabs = prefabs;
		}

		public ObjectPool(Transform thisTransform, params GameObject[] prefabs) {
			_thisTransform = thisTransform;
			_prefabs = new T[prefabs.Length];
			_projectilesPoolParent = new GameObject("<ObjectPool>");
			for (int i = 0; i < prefabs.Length; i++) {
				var prefab = prefabs[i];
				var tempObj = GameObject.Instantiate<GameObject>(prefab);
				tempObj.name = "tempPrefab[" + prefab.name + "]";
				tempObj.transform.SetParent(_projectilesPoolParent.transform);
				tempObj.SetActive(false);
				_prefabs[i] = tempObj.AddComponent<T>();
			}
		}

		public T GetObject() {
			if (_poolObjects.Count > 0) {
				var lastIndex = _poolObjects.Count - 1;
				var last = _poolObjects[lastIndex];
				_poolObjects.RemoveAt(lastIndex);
				ActiveObjects.Add(last);
				last.gameObject.SetActive(true);
				last.transform.position = _thisTransform.position;
				return last;
			}
			else {
				var prefab = _prefabs[UnityEngine.Random.Range(0, _prefabs.Length - 1)];
				var newProj = GameObject.Instantiate<T>(prefab);
				newProj.gameObject.SetActive(true);
				ActiveObjects.Add(newProj);
				newProj.transform.position = _thisTransform.position;
				newProj.transform.SetParent(_projectilesPoolParent.transform);
				return newProj;
			}
		}

		public void ReturnObject(T obj) {
			ActiveObjects.Remove(obj);
			_poolObjects.Add(obj);
			obj.gameObject.SetActive(false);
		}

	}
}
