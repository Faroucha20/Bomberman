using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DeplacementJoueur : MonoBehaviour
{
    [Header("Paramètres")]
    public float vitesse = 5f;

    private Rigidbody rb;
    private Animator anim; // 1. On déclare l'Animator
    private Vector3 mouvement;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>(); // 2. On récupère le composant Animator
    }

    void Update()
    {
        float axeHorizontal = Input.GetAxisRaw("Horizontal");
        float axeVertical = Input.GetAxisRaw("Vertical");

        mouvement = new Vector3(axeHorizontal, 0f, axeVertical).normalized;

        // 3. On informe l'Animator si le joueur touche les flèches ou non
        if (anim != null)
        {
            bool bouge = mouvement != Vector3.zero;
            anim.SetBool("EstEnMouvement", bouge);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = mouvement * vitesse;

        if (mouvement != Vector3.zero)
        {
            transform.forward = mouvement;
        }
    }
}