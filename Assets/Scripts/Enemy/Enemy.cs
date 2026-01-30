using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class Enemy : MonoBehaviour
{
    #region Perseguição do Jogador
    
    [SerializeField] private float speedTarget;
    [SerializeField] private float distanceMin;
    [SerializeField] private Rigidbody rig;

    [SerializeField] private LayerMask playerMasck;


    #endregion Perseguição do Jogador

    [SerializeField]
    private Transform Target;

    [SerializeField] private float lookRadius;

    // Patrulha do Inimigo (Paths();)
    public float speed;

    private int index;

    public List<Transform> paths = new List<Transform>();


    // Update is called once per frame
    void Update()
    {
        SearchPlayer();
        if (this.Target != null)
        {
            ChaseTarget();
        }
        else
        {
            Paths();
        }
    }

    private void ChaseTarget()
    {
        Vector3 positionTarget = this.Target.position;
        Vector3 positionCurrent = this.transform.position;

        float distance = Vector3.Distance(positionCurrent, positionTarget);
        if (distance >= this.distanceMin)
        {
            Vector3 direction = positionTarget - positionCurrent;
            direction = direction.normalized;

            this.rig.linearVelocity = (this.speedTarget * direction);
        }
    }

    private void Paths()
    {
        transform.position = Vector3.MoveTowards(transform.position, paths[index].position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, paths[index].position) < 0.1f)
        {
            if (index < paths.Count - 1)
            {
                index++;
            }
            else
            {
                index = 0;
            }

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, this.lookRadius);
    }

    private void SearchPlayer()
    {
        Collider[] colision = Physics.OverlapSphere(this.transform.position, this.lookRadius, playerMasck);
        if(colision != null && colision.Length > 0)
        {
            this.Target = colision[0].transform;
        }
        else
        {
            this.Target = null;
        }


       
    }

}
