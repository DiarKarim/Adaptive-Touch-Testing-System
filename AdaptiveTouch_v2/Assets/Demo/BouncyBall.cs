using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Syntacts;

public class BouncyBall : MonoBehaviour
{
    public SyntactsHub syntacts;   
    private Rigidbody rb;
    private MeshRenderer meshrend; 
    private Color origColor;
    private Color newColor = new Color(0f, 1f, 0f, 1f);

    public int collisionChannel = 0;
    public int velocityChannel = 1;

    public float collisionFreq = 500;
    public float velocityFreq = 200;


    // Start is called before the first frame update
    void Start()
    {
        syntacts.session.Play(velocityChannel, new Sine(velocityFreq) * new Sine(5));
        rb = GetComponent<Rigidbody>();
        meshrend = GetComponent<MeshRenderer>();
        origColor = meshrend.material.color; 
    }

    // Update is called once per frame
    void Update()
    {
        syntacts.session.SetPitch(velocityChannel, 1 + rb.velocity.magnitude* 0.1);
    }

    void OnCollisionEnter(Collision col) {
        Signal collision = new Square(collisionFreq) * new ASR(0.05, 0.05, 0.05);
        syntacts.session.Play(collisionChannel, collision);
        meshrend.material.color = newColor; 
    }

    private void OnCollisionExit(Collision col)
    {
        Invoke("ResetColor", 0.3f);
    }

    void ResetColor()
    {
        meshrend.material.color = origColor;
    }
}
