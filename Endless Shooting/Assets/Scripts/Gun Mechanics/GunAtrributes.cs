﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct GunAtrributes
{
    public string Name;
    public int ClipSize;
    public int MaxAmmo;
    public int CurrentAmmoInCip;
    public int Damage;
    public float RateOfFire;
    public float Range;
    public GunType Type;
    [HideInInspector] public Animation Animation;
    [HideInInspector] public Animator Animator;
    public AudioSource ShotSound;
    public AudioSource ReloadSound;
    public Transform ShotPoint;
    public GameObject Crosshair;
    public GameObject[] VFX;

}
