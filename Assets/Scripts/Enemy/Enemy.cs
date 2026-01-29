using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.Controls;

public class Enemy : MonoBehaviour
{
    private float lookRadius;

    public float speed;

    private int index;

    public List<Transform> paths = new List<Transform>();


    // Update is called once per frame
    void Update()
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

}
