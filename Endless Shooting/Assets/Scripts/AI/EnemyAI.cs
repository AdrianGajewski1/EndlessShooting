﻿using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.AI;
using UnityStandardAssets.Characters.FirstPerson;

public class EnemyAI : AI
{
    [Header("Field of view")]
    [SerializeField] float maxRadius;
    [SerializeField] float maxAngle;
    [SerializeField] LayerMask mask;
    [SerializeField] LayerMask raycastHitmask;

    [Header("Attacking")] 
    [SerializeField] GunAtrributes Atributes;
    [SerializeField] ParticleSystem muzzle;
    [SerializeField] bool InAttackMode = false;
    [SerializeField] float rotateSpeed;
    private float _timeToFireAllowed = 0f;
    private Transform targetAI;
    private Transform targetPlayer;
    public static int score;
    public int Deaths;

    [Header("Navigation")]
    NavMeshAgent navAgent;
    public Transform[] waypoints;

    void Start()
    {
        Atributes.MaxAmmo = int.MaxValue;
        navAgent = GetComponent<NavMeshAgent>();
        Atributes.Animator = GetComponentInChildren<Animator>();
        SetDestination(ref navAgent, waypoints);
    }
    void OnDrawGizmos()
    {
        DrawGizmos(transform, maxRadius, maxAngle );
    }
    public int GetScore() => score;
    public int GetNumberOfDeaths() => Deaths;
    void FixedUpdate()
    {
        targetAI = ScanForTarget<FriendAI>(transform, mask, maxRadius, maxAngle);
        targetPlayer = ScanForTarget<FirstPersonController>(transform, mask, maxRadius, maxAngle);

        if(targetAI != null)
            RotateToTarget(transform, targetAI, rotateSpeed);

        if(targetPlayer != null)
        {
            RotateToTarget(transform, targetPlayer, rotateSpeed);
            Atributes.Animator.SetBool("Attacking", true);
            navAgent.speed = 0;
        }
        else
        {
            Atributes.Animator.SetBool("Attacking", false);
            navAgent.speed = 3.5f;
        }


        Shoot();
    }

    void Update()
    {
        if(Atributes.CurrentAmmoInCip == 0)
            StartCoroutine(Reload());

        if (!navAgent.pathPending)
        {
            if (navAgent.remainingDistance <= navAgent.stoppingDistance)
            {
                if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                {
                    SetDestination(ref navAgent, waypoints);
                }
            }
        }
    }

    public override void Shoot()
    {
        if(Time.time >= _timeToFireAllowed  && Atributes.CurrentAmmoInCip >= 1)
        {
            RaycastHit hitInfo;
            if(Physics.Raycast(Atributes.ShotPoint.position, Atributes.ShotPoint.TransformDirection(Vector3.forward), out hitInfo, Atributes.Range, raycastHitmask))
            {
                if(hitInfo.transform != null) 
                {   
                    if(hitInfo.transform.CompareTag("PlayerTeam") || hitInfo.transform.CompareTag("Player"))
                    {
                        Atributes.CurrentAmmoInCip -= 1;
                        _timeToFireAllowed = Time.time + 1 / Atributes.RateOfFire;
                        Atributes.ShotSound.Play();
                        muzzle.Play();
                        GiveDamage<PlayerHealth>(ref hitInfo, Atributes.Damage);
                        GiveDamage<AIHealth>(ref hitInfo, Atributes.Damage);
                        if(hitInfo.transform.GetComponent<IHealth>().IsDead() == true)
                        {
                            score += 1;
                            return;
                        }
                    }   
                }
            }   
        }    
    }

    public override IEnumerator Reload()
    {
        if(Atributes.MaxAmmo > 0)
        {
            if(Atributes.MaxAmmo >= 1)
            {
                Atributes.ReloadSound.Play();
                GetComponent<EnemyAI>().enabled = false;
                yield return new WaitForSeconds(4f);
                GetComponent<EnemyAI>().enabled = true;
                if(Atributes.MaxAmmo >= 1)
                {
                    if(Atributes.CurrentAmmoInCip > 0)
                    {
                        var currentAmmo = Atributes.CurrentAmmoInCip;
                        var ammoToAdd = Atributes.ClipSize - currentAmmo;
                        Atributes.MaxAmmo -= ammoToAdd;
                        Atributes.CurrentAmmoInCip += ammoToAdd;
                    }
                    
                    if(Atributes.CurrentAmmoInCip == 0)
                    {
                        var ammoToAdd = Atributes.ClipSize;
                        Atributes.MaxAmmo -= ammoToAdd;
                        Atributes.CurrentAmmoInCip += ammoToAdd;
                    }  
                }
            }  
        }
    }
}
