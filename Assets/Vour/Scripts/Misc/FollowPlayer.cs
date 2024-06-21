using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CrizGames.Vour
{
    public class FollowPlayer : MonoBehaviour
    {
        [SerializeField] private Vector3 playerOffsetFwd = Vector3.forward;
        [Space]
        [SerializeField] private float maxRotationDiff = 30f;
        [SerializeField] private float rotationSmoothTime = 0.1f;
        private float currentSurroundRot, targetSurroundRot, rotVel;

        private Transform playerT;


        private void Start()
        {
            playerT = PlayerBase.Instance.cam.transform;

            transform.SetParent(new GameObject(name + " Parent").transform, false);
            transform.localPosition = playerOffsetFwd;
        }

        private void LateUpdate()
        {
            Move();
            Rotate();
        }

        private float lastPlayerYRot = 0f;
        private void Rotate()
        {
            float playerYRot = playerT.eulerAngles.y;

            // If the 0 to 360 flip happened
            if (lastPlayerYRot < 90f && playerYRot > 270f)
                currentSurroundRot += 360f;
            else if (lastPlayerYRot > 270f && playerYRot < 90f)
                currentSurroundRot -= 360f;

            // Rotate around player
            targetSurroundRot = Mathf.Clamp(currentSurroundRot, playerYRot - maxRotationDiff, playerYRot + maxRotationDiff);
            currentSurroundRot = Mathf.SmoothDampAngle(currentSurroundRot, targetSurroundRot, ref rotVel, rotationSmoothTime);
            transform.parent.eulerAngles = Vector3.up * currentSurroundRot;

            // Look at player
            transform.LookAt(playerT);
            transform.Rotate(Vector3.up * 180f);

            lastPlayerYRot = playerYRot;
        }

        private void Move()
        {
            transform.parent.position = playerT.position;
        }
    }
}