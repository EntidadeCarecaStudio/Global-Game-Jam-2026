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
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstacleMask; // Camada para obstáculos que bloqueiam visão

    #endregion Perseguição do Jogador

    [SerializeField] private Transform target;
    [SerializeField] private float lookRadius;
    [SerializeField] private float raycastHeightOffset = 0.5f; // Ajuste para altura do raycast
    [SerializeField] private CharacterStats stats;


    // Patrulha do Inimigo (Paths();)
    public float speed;
    private int index;
    public List<Transform> paths = new List<Transform>();

    //Vida Do Inimigo para Morrer
    public float health;
    public bool isDead;


    // Animação do Inimigo Pegando Componente de outro Script (AnimationControl();)
    [SerializeField] private AnimationControl animControl;

    void Update()
    {
        if (!isDead)
        {
            SearchPlayer();
            if (this.target != null)
            {
                ChaseTarget();
            }
            else
            {
                Paths();
            }
        }


        
    }

    private void ChaseTarget()
    {
        Vector3 positionTarget = this.target.position;
        Vector3 positionCurrent = this.transform.position;

        float distance = Vector3.Distance(positionCurrent, positionTarget);
        if (distance >= this.distanceMin)
        {
            Vector3 direction = positionTarget - positionCurrent;
            direction = direction.normalized;

            this.rig.linearVelocity = (this.speedTarget * direction);

            // Opcional: rotacionar o inimigo na direção do movimento
            if (direction != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
                animControl.PlayAnim(1);
            }
        }
        else
        {
            // Parar quando estiver próximo o suficiente
            this.rig.linearVelocity = Vector3.zero;
            animControl.PlayAnim(2);
        }
    }

    private void Paths()
    {
        if (paths.Count == 0) return;

        transform.position = Vector3.MoveTowards(transform.position, paths[index].position, speed * Time.deltaTime);

        // Rotacionar em direção ao próximo ponto
        Vector3 directionToPath = (paths[index].position - transform.position).normalized;
        if (directionToPath != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToPath);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 3f);
            animControl.PlayAnim(1);
        }

        if (Vector3.Distance(transform.position, paths[index].position) < 0.1f)
        {
            if (paths.Count > 1)
            {
                // Seleciona aleatoriamente um novo ponto, podendo ser qualquer um
                int newIndex;
                do
                {
                    newIndex = Random.Range(0, paths.Count);
                } while (newIndex == index && paths.Count > 1); // Evita repetir o mesmo ponto

                index = newIndex;
            }
        }
    }

    private void SearchPlayer()
    {
        Collider[] collisions = Physics.OverlapSphere(this.transform.position, this.lookRadius, playerMask);

        if (collisions.Length > 0)
        {
            Transform playerTransform = collisions[0].transform;

            // Verificar linha de visão com Raycast
            Vector3 raycastOrigin = transform.position + Vector3.up * raycastHeightOffset;
            Vector3 raycastTarget = playerTransform.position + Vector3.up * raycastHeightOffset;
            Vector3 directionToPlayer = raycastTarget - raycastOrigin;

            RaycastHit hit;

            // Disparar raycast na direção do jogador
            if (Physics.Raycast(raycastOrigin, directionToPlayer, out hit, lookRadius))
            {
                // Verificar se o raycast atingiu o jogador
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    this.target = playerTransform;
                    return; // Jogador detectado, sair do método
                }
            }
        }

        // Se chegou aqui, não detectou jogador ou perdeu a visão
        this.target = null;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, this.lookRadius);

        // Desenhar raycast quando houver target (para debug)
        if (Application.isPlaying && target != null)
        {
            Gizmos.color = Color.red;
            Vector3 raycastOrigin = transform.position + Vector3.up * raycastHeightOffset;
            Gizmos.DrawLine(raycastOrigin, target.position + Vector3.up * raycastHeightOffset);
        }
    }

}
