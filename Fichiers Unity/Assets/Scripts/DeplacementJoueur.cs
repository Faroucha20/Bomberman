using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DeplacementJoueur : MonoBehaviour
{
    // On peut retirer la variable "vitesse" d'ici, car on va la chercher dans l'inventaire !

    private Rigidbody rb;
    private Animator anim;
    private Vector3 mouvement;
    
    // 1. On crée une place pour stocker l'inventaire
    private Inventaire monInventaire;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        
        // 2. On récupère le script Inventaire qui est attaché au Bomberman
        monInventaire = GetComponent<Inventaire>(); 
    }

    void Update()
    {
        float axeHorizontal = Input.GetAxisRaw("Horizontal");
        float axeVertical = Input.GetAxisRaw("Vertical");

        mouvement = new Vector3(axeHorizontal, 0f, axeVertical).normalized;

        if (anim != null)
        {
            bool bouge = mouvement != Vector3.zero;
            anim.SetBool("EstEnMouvement", bouge);
        }
    }

    void FixedUpdate()
    {
        // 3. On demande à l'inventaire quelle est la vitesse actuelle
        // (Le "? monInventaire.vitesseJoueur : 5f" est une sécurité au cas où l'inventaire n'existe pas)
        float vitesseActuelle = (monInventaire != null) ? monInventaire.vitesseJoueur : 5f;

        // 4. On applique la bonne vitesse au mouvement !
        rb.linearVelocity = mouvement * vitesseActuelle;

        if (mouvement != Vector3.zero)
        {
            transform.forward = mouvement;
        }
    }
}