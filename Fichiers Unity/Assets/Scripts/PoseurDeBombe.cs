using UnityEngine;
using Unity.Netcode; // --- NOUVEAU : Import de la librairie réseau ---

// --- MODIFICATION : On hérite de NetworkBehaviour ---
public class PoseurDeBombe : NetworkBehaviour
{
    [Header("Paramètres")]
    public GameObject prefabBombe; // Le modèle de la bombe à faire apparaître

    // Référence vers l'inventaire du joueur
    private Inventaire monInventaire;

    void Start()
    {
        // Au démarrage, on récupère le script Inventaire qui est sur le même objet
        monInventaire = GetComponent<Inventaire>();
    }

    void Update()
    {
        // --- SÉCURITÉ RÉSEAU ---
        // On ne lit la touche Espace QUE si ce script appartient à NOTRE personnage local.
        if (!IsOwner) return;

        // 1. On écoute la touche Espace
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // 2. On demande la permission à l'inventaire avant de poser
            if (monInventaire != null && monInventaire.PeutPoserBombe())
            {
                // 3. Récupérer la position exacte du joueur
                Vector3 positionJoueur = transform.position;

                // 4. Calculer le centre de la case (Arrondir les positions X et Z)
                float xArrondi = Mathf.Round(positionJoueur.x);
                float zArrondi = Mathf.Round(positionJoueur.z);

                // 5. Définir la position finale de la bombe
                Vector3 positionSurGrille = new Vector3(xArrondi, -0.75f, zArrondi); 

                // 6. On prévient notre inventaire local qu'on a posé une bombe
                monInventaire.AjouterBombeSurCarte();

                // 7. On demande poliment au Serveur de faire apparaître la bombe pour tout le monde
                PoserUneBombeServerRpc(positionSurGrille, monInventaire.porteeExplosion);
            }
        }
    }

    // --- NOUVELLE FONCTION RÉSEAU ---
    // Cette fonction ne s'exécute QUE sur le Serveur
    [ServerRpc]
    void PoserUneBombeServerRpc(Vector3 positionSurGrille, int portee)
    {
        // Le Serveur crée la bombe
        GameObject nouvelleBombe = Instantiate(prefabBombe, positionSurGrille, Quaternion.identity);

        // On configure la bombe
        Bombe scriptBombe = nouvelleBombe.GetComponent<Bombe>();
        if (scriptBombe != null)
        {
            scriptBombe.porteeExplosion = portee; 
            // Le serveur relie la bombe à l'inventaire du joueur (côté serveur) pour le remboursement
            scriptBombe.inventaireJoueur = monInventaire; 
        }

        // Le Serveur annonce la bombe à tout le réseau
        NetworkObject netObj = nouvelleBombe.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }
}