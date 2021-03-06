﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SniperGun : MonoBehaviour, IGun
{
    public GunAtrributes Atributes;
    Ray ray;
    RaycastHit hit;
    string tag = string.Empty;
    float timeToFireAllowed;
    private const float NORMAL_FIELD_OF_VIEW = 60f;
    [SerializeField] float scopedFOV;
    [SerializeField] ParticleSystem muzzle;
    [SerializeField] GameObject scopeOverlay;
    [SerializeField] GameObject weaponCam;
    [SerializeField] Camera mainCam;

    public void GiveDamage(ref RaycastHit ray, int damageAmount)
    {
        if(ray.transform != null)
        {
            if(ray.transform.GetComponent<AIHealth>() != null)
            {
                ray.transform.GetComponent<AIHealth>().GetDamage(damageAmount);
            }
        }
    }



    public IEnumerator Reload()
    {
        if(InputController.Reload_Button)
        {
            if(Atributes.MaxAmmo >= 1 && Atributes.Animator.GetBool("Scope") == false)
            {
                Atributes.ReloadSound.Play();
                Atributes.Animator.SetBool("Reload",true);
                Atributes.Animator.SetBool("Idle",false);
                GetComponentInParent<SniperGun>().enabled = false;
                yield return new WaitForSeconds(3f);
                GetComponentInParent<SniperGun>().enabled = true;   
                Atributes.Animator.SetBool("Reload",false);
                Atributes.Animator.SetBool("Idle",true);
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
    async void Aim()
    {
        if(InputController.Right_Mouse)
        {   
            scopedFOV = 15f;
            Atributes.Animator.SetBool("Scope", true);
            await Task.Delay(250);
            weaponCam.SetActive(false);
            scopeOverlay.SetActive(true);
            mainCam.fieldOfView = scopedFOV;
        }

        if (Input.GetAxis("Mouse ScrollWheel") > 0f ) // forward
        {
            scopedFOV--;
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0f ) // backwards
        {
            scopedFOV++;
        }

        if(!InputController.Right_Mouse)
        {
            Atributes.Animator.SetBool("Scope", false);
            scopeOverlay.SetActive(false);
            weaponCam.SetActive(true);
            mainCam.fieldOfView = NORMAL_FIELD_OF_VIEW;
        }
    }
    public void Shoot()
    {
        if(InputController.Left_Mouse && Atributes.CurrentAmmoInCip >= 1 && Time.time >= timeToFireAllowed )
        {
            Atributes.Animator.SetBool("Shoot", true);
            Atributes.Animator.SetBool("Idle", false);
            Atributes.CurrentAmmoInCip -= 1;
            timeToFireAllowed = Time.time + 1 / Atributes.RateOfFire;
            Atributes.ShotSound.Play();
            SpawnProjectile();

            if(tag == "Enemy")
            {
                GiveDamage(ref hit, Atributes.Damage);
            }
            SpawnProjectile();
            SpawnHitEffect();
        }
        else
        {
            Atributes.Animator.SetBool("Shoot", false);
            Atributes.Animator.SetBool("Idle", true);  
        }    
            
    }

    void SpawnHitEffect()
    {

    }
    public void SpawnProjectile()
    {
        muzzle.Play();
    }

    void Start()
    {
        Atributes.Animator = GetComponentInParent<Animator>();
        Atributes.Crosshair.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        UIAmmoController.Type = Atributes.Type;
        Atributes.Crosshair.SetActive(true);
        Aim();
        Shoot();
        StartCoroutine(Reload());
        SetInput();
    }

    void FixedUpdate()
    {
        ray = Camera.main.ScreenPointToRay(Atributes.Crosshair.transform.position);

        if(Physics.Raycast(ray, out hit, Atributes.Range))
        { 
        }

        if(hit.transform != null)
        {
            if(hit.transform.tag != null)
            {
                tag = hit.transform.tag;
            }
        }  
    }

    void SetInput()
    {
        InputController.Left_Mouse = Input.GetButtonDown("Fire1");
        InputController.Right_Mouse = Input.GetMouseButton(1);
        InputController.Reload_Button = Input.GetButtonDown("Reload");
    }

}
