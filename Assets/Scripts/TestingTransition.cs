using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestingTransition : MonoBehaviour
{
    public Animator playerAnimator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown("space"))
        {
            playerAnimator.SetBool("Death", true);
        }
    }
}