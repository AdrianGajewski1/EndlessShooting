﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;

public class EnemyAI : AI
{
    [Header("Field of view")]
    [SerializeField] float maxRadius;
    [SerializeField] float maxAngle;
    [SerializeField] LayerMask mask;


    void OnDrawGizmos()
    {
        DrawGizmos(transform, maxRadius, maxAngle );
    }

    void FixedUpdate()
    {
        ScanForTarget<FirstPersonController>(transform, mask, maxRadius, maxAngle);
        ScanForTarget<FriendAI>(transform, mask, maxRadius, maxAngle);
    }
}
