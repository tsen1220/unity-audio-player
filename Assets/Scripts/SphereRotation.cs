using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereRotation : MonoBehaviour
{
    private void Update()
    {
        gameObject.transform.eulerAngles += new Vector3(0, 1, 0);
    }
}
