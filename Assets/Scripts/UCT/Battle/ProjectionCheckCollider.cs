using System.Collections.Generic;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Battle
{
    /// <summary>
    ///     投影原先心的判定。
    ///     如果你直接把弹幕放在原先心的位置（负一千），可能会导致显示问题。
    /// </summary>
    public class ProjectionCheckCollider : ObjectPool
    {
        private GameObject _canvasBoxProjectionSet;

        private readonly List<GameObject> _checkColliders = new();

        private readonly List<GameObject> _sets = new();

        // Start is called before the first frame update
        private void Start()
        {
            _canvasBoxProjectionSet = GameObject.Find("CanvasBoxProjectionSet");

            poolObject = (GameObject)Resources.Load("Prefabs/CheckCollider");
            for (var i = 0; i < _canvasBoxProjectionSet.transform.childCount; i++)
            {
                _sets.Add(_canvasBoxProjectionSet.transform.GetChild(i).gameObject);
                var obj = GetFromPool<Transform>();
                _checkColliders.Add(obj.gameObject);
            }
        }

        // Update is called once per frame
        private void Update()
        {
            var relative = (Vector2)MainControl.Instance.battlePlayerController.transform.position -
                           MainControl.Instance.battlePlayerController.sceneDrift;
            for (var i = 0; i < _sets.Count; i++)
            {
                var convert = _sets[i].transform.position + _sets[i].transform.rotation * relative;
                _checkColliders[i].transform.position = convert - _canvasBoxProjectionSet.transform.position;
                _checkColliders[i].transform.rotation = _sets[i].transform.rotation;
            }
        }
    }
}