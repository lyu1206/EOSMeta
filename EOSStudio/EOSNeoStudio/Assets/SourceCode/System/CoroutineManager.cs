using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EosPlayer
{
    public class CoroutineManager 
    {
        private static int coroutineuid = 0;
        private Dictionary<int,IEnumerator> _routines = new Dictionary<int, IEnumerator>();
        private Dictionary<int, IEnumerator> _startroutines = new Dictionary<int, IEnumerator>();
        private Coroutine _updator;
        private List<int> lateremoveCoroutineIDs = new List<int>();
        private MonoBehaviour _adapter;
        public CoroutineManager(MonoBehaviour adapter)
        {
            _adapter = adapter;
        }
        private IEnumerator UpdateCoRoutines()
        {
            while (_routines.Count>0)
            {
                for (int i = 0; i < lateremoveCoroutineIDs.Count; i++)
                {
                    _routines.Remove(lateremoveCoroutineIDs[i]);
                }

                lateremoveCoroutineIDs.Clear();
                var routines = new List<IEnumerator>(_routines.Values);
                var keys = new List<int>(_routines.Keys);
                for (int i=0;i<routines.Count;i++)
                {
                    var routine = routines[i];
                    var doit = routine.MoveNext();
                    yield return routine.Current;
                    if (!doit)
                    {
                        lateremoveCoroutineIDs.Add(keys[i]);
                    }
                }
                yield return 0;
            }
            _updator = null;
        }

        public int RegistCoroutine(IEnumerator routine)
        {
            var id = coroutineuid++;
            _routines.Add(id,routine);
            if (_updator == null)
            {
                _updator = _adapter.StartCoroutine(UpdateCoRoutines());
            }

            return id;
        }

        public void UnRegistCoroutine(int routineID)
        {
            lateremoveCoroutineIDs.Add(routineID);
        }
        public int NowID;
        public Coroutine OnCoroutineStart(IEnumerator routine)
        {
            var id = coroutineuid++;
            _startroutines.Add(id, routine);
            NowID = id;
            return _adapter.StartCoroutine(routine);
        }
        public void OnStopCoroutine(int id)
        {
            if (!_startroutines.ContainsKey(id))
                return;
            _adapter.StopCoroutine(_startroutines[id]);
            _startroutines.Remove(id);
        }
    }
}