using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnFreezeOnCollid : MonoBehaviour
{
    public Rigidbody2D rb;

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            rb.constraints = RigidbodyConstraints2D.None;
        }
    }
}
