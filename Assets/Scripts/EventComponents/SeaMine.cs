﻿using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class SeaMine : MonoBehaviour, IHandleInteraction
{
    private static bool firstMineEncounter = true;
    
    private bool interactable = true;
    static PlayerStats playerStats;
    private void Update()
    {
        if (PlayerOnMine())
        {
            Explode();
        }
    }
    
    public bool Interactable => interactable;
    
    public IEnumerator HandleEventInteraction()
    {

        if (!interactable) yield break;
        
        EventInteraction interaction = FindObjectOfType<EventInteraction>();
        if (interaction == null) yield break;
        
        GameUIManager.EventDisp(0);
        if (playerStats == null) {
            playerStats = FindObjectOfType<PlayerStats>();
        }
        bool eventHandled = false;
        
        Action onAccept = () => 
        {
            eventHandled = true;
        };

        Action onDeny = () =>
        {
            eventHandled = true;
        };

        bool explode;

        if (firstMineEncounter)
        {
            explode = false;
            firstMineEncounter = false;
        }
        else
        {
            explode = Random.value < 0.5f;
        }

        if (explode)
        {
            interaction.SendSeaMineInteractionMessage(true, 0,
                () => {
                    onAccept.Invoke();
                    DealDamage();
                    gameObject.SetActive(false);
                }, onDeny);
        }
        else
        {
            int scrapToCollect = Random.Range(1, 4);
            interaction.SendSeaMineInteractionMessage(false, scrapToCollect,
                () => {
                    onAccept.Invoke();
                    
                    if (playerStats != null)
                    {
                        playerStats.scrapCount += scrapToCollect;
                        GameUIManager.UpdateScrap(playerStats.scrapCount);
                        gameObject.SetActive(false);
                    }
                }, onDeny);
        }
        
        while (!eventHandled)
        {
            yield return null;
        }
        
        GameUIManager.EventDisp(-1);
        interactable = false;
    }
    
    private void DealDamage()
    {
        float leakIncrease = Random.Range(30, 60);
        GameUIManager.UpdateLeak(GameUIManager.instance.leak.value + leakIncrease);
        
        KillEnemiesInRange();
    }
    
    public void Explode()
    {
        DealDamage();
        gameObject.SetActive(false);
    }
    
    private void KillEnemiesInRange()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(3f, 3f, 3f), transform.rotation);
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Enemy"))
            {
                col.GetComponent<Enemy>().Kill();
            }
        }
    }
    
    private bool PlayerOnMine()
    {
        Collider[] colliders = Physics.OverlapBox(transform.position, new Vector3(0.5f, 0.5f, 0.5f), transform.rotation);
        
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("Player") && !interactable)
            {
                PlayerMovement player = col.GetComponent<PlayerMovement>();
                if (player.transform.position == transform.position)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(1, 1, 1));
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(0.5f, 0.5f, 0.5f));
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, new Vector3(3f, 3f, 3f));
    }
}
