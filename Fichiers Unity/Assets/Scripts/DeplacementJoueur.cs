using UnityEngine;
using System.Collections; // Indispensable pour utiliser les Coroutines

[RequireComponent(typeof(Rigidbody))]
public class DeplacementJoueur : MonoBehaviour
{
    private Rigidbody rb;
    private Animator anim;
    private Vector3 mouvement;
    private Inventaire monInventaire;

    // --- NOUVEAU : On ajoute une variable pour bloquer le joueur ---
    private bool estMort = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        monInventaire = GetComponent<Inventaire>(); 
    }

    void Update()
    {
        // Si le joueur est mort, on ignore ses touches !
        if (estMort) return;

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
        // Si le joueur est mort, on annule les déplacements physiques
        if (estMort) return;

        float vitesseActuelle = (monInventaire != null) ? monInventaire.vitesseJoueur : 5f;

        rb.linearVelocity = mouvement * vitesseActuelle;

        if (mouvement != Vector3.zero)
        {
            transform.forward = mouvement;
        }
    }

    void OnTriggerEnter(Collider autre)
    {
        if (autre.CompareTag("Explosion") || autre.CompareTag("Monstre"))
        {
            Mourir();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Explosion") || collision.gameObject.CompareTag("Monstre"))
        {
            Mourir();
        }
    }

    void Mourir()
    {
        // Si on est déjà mort, on ne relance pas la fonction (pour éviter les bugs si touché 2 fois de suite)
        if (estMort) return; 

        Debug.Log("Le Bomberman a été éliminé !");
        
        // On lance la séquence animée
        StartCoroutine(SequenceMort());
    }

    // --- NOUVELLE COROUTINE ---
   IEnumerator SequenceMort()
    {
        estMort = true; 
        rb.linearVelocity = Vector3.zero;

        if (anim != null) anim.SetTrigger("Mort");

        yield return new WaitForSeconds(1.5f);
        yield return new WaitForSeconds(2f);

        // --- ON APPELLE LE SCRIPT JEU ---
        Jeu scriptJeu = FindFirstObjectByType<Jeu>();
        
        if (scriptJeu != null)
        {
            scriptJeu.AfficherGameOver(); 
        }
        else
        {
            Debug.LogError("🚨 ERREUR : Le code ne trouve pas le script Jeu !");
        }
        // --------------------------------
        
        Destroy(gameObject);
    }
}