using UnityEngine;

public class ProjMove : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private float fireRate;
    void Start()
    {
        
    }

    void Update()
    {
        if(speed != 0)
        {
            transform.position += transform.forward * (speed * Time.deltaTime);
        }
        else
        {
            Debug.Log("No speed");
        }
    }
}
