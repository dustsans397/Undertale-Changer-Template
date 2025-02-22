using System;
using UCT.Global.Core;
using UnityEngine;

namespace UCT.Other
{
    public class FollowSth : MonoBehaviour
    {
        public FollowTarget followTarget;
        public GameObject sth;
        public bool followPosition;
        public Vector3 positionAdd;
        public bool followRotation;
        public Vector3 rotationAdd;
        public bool followLocalScale;
        public Vector3 localScaleAdd;

        public enum FollowTarget
        {
            Null,
            Player,
            MainCamera
        }

        private void Start()
        {
            switch (followTarget)
            {
                case FollowTarget.Null:
                    break;
                case FollowTarget.Player:
                    SetSth(MainControl.overworldPlayerBehaviour.gameObject);
                    break;
                case FollowTarget.MainCamera:
                    SetSth(MainControl.Instance.mainCamera.gameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void SetSth(GameObject item)
        {
            if (item)
            {
                sth = item;
            }
            else
            {
                throw new ArgumentNullException();
            }
        }

        private void Update()
        {
            if (!sth)
            {
                return;
            }

            if (followPosition)
            {
                transform.position = sth.transform.position + positionAdd;
            }

            if (followRotation)
            {
                transform.rotation = sth.transform.rotation * Quaternion.Euler(rotationAdd);
            }

            if (followLocalScale)
            {
                transform.localScale = sth.transform.localScale + localScaleAdd;
            }
        }
    }
}