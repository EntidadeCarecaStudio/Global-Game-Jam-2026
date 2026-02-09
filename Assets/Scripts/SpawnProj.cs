using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class SpawnProj : MonoBehaviour
{
    [SerializeField] private GameObject firepoint;
    [SerializeField] private List<GameObject> vfx = new List<GameObject>();
    [SerializeField] private RotateToMouse rotateToMouse;
    private GameObject effectToSpawn;
    void Start()
    {
        effectToSpawn = vfx[0];
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            SpawnVFX();
        }
    }

    void SpawnVFX()
    {
        GameObject vfx;
        if(firepoint != null)
        {
            vfx = Instantiate(effectToSpawn, firepoint.transform.position, Quaternion.identity);
            if(rotateToMouse != null)
            {
                vfx.transform.localRotation = rotateToMouse.GetRotation();
            }

        }
        else
        {
            Debug.Log("No fire point");
        }
    }
}
