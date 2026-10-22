using UnityEngine;
using System.Collections; // Indispensable pour utiliser les Coroutines
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody))]
public class DeplacementJoueur : NetworkBehaviour
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
        if (!IsOwner) return; // Vérifie si ce script appartient au joueur local

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

    public override void OnNetworkSpawn()
    {
        // 1. DÉPLACEMENT (Autorité du Serveur)
        // Le Serveur téléporte ce nouveau joueur à sa place attitrée
        if (IsServer)
        {
            GenerateurMap map = FindFirstObjectByType<GenerateurMap>();
            if (map != null)
            {
                // Netcode s'occupera de synchroniser cette nouvelle position chez tout le monde
                transform.position = map.ObtenirPointDeDepart(OwnerClientId);
            }
        }

        // 2. INTERFACE (Action Locale)
        // Si c'est notre personnage, on connecte son sac de bombes à l'écran
        if (IsOwner)
        {
            Jeu interfaceUI = FindFirstObjectByType<Jeu>(FindObjectsInactive.Include);
            if (interfaceUI != null)
            {
                // MODIFICATION ICI : On récupère spécifiquement le composant Inventaire
                interfaceUI.AssignerJoueur(GetComponent<Inventaire>());
            }
        }
    }
}