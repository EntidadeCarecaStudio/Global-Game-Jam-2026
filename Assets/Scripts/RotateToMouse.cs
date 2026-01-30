using UnityEngine;

public class RotateToMouse : MonoBehaviour
{
    [SerializeField] private Camera cam;
    [SerializeField] private float maxLen;

    private Ray rayMouse;
    private Vector3 pos;
    private Vector3 dir;
    private Quaternion rot;
 
    void Update()
    {
        if(cam != null)
        {
            RaycastHit hit;
            var mousePos = Input.mousePosition;
            rayMouse = cam.ScreenPointToRay(mousePos);
            if(Physics.Raycast(rayMouse.origin, rayMouse.direction, out hit, maxLen))
            {
                RotateToMouseDirection(gameObject, hit.point);
            }
            else
            {
                var pos = rayMouse.GetPoint(maxLen);
                RotateToMouseDirection(gameObject, pos);
            }
        }
        else
        {
            Debug.Log("No camera");
        }
    }

    void RotateToMouseDirection(GameObject go, Vector3 dest)
    {
        dir = dest - go.transform.position;
        rot = Quaternion.LookRotation(dir);
        go.transform.localRotation = Quaternion.Lerp(go.transform.rotation, rot, 1);
    }

    public Quaternion GetRotation()
    {
        return rot;
    }
}
