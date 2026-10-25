using UnityEngine;
using Unity.Netcode;

public class PoseurDeBombe : NetworkBehaviour
{
    [Header("Paramètres")]
    public GameObject prefabBombe;

    private Inventaire monInventaire;

    void Start()
    {
        monInventaire = GetComponent<Inventaire>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (monInventaire != null && monInventaire.PeutPoserBombe())
            {
                Vector3 positionJoueur = transform.position;
                float xArrondi = Mathf.Round(positionJoueur.x);
                float zArrondi = Mathf.Round(positionJoueur.z);
                Vector3 positionSurGrille = new Vector3(xArrondi, -0.75f, zArrondi); 

                monInventaire.AjouterBombeSurCarte();

                // On envoie la demande au serveur
                PoserUneBombeServerRpc(positionSurGrille, monInventaire.porteeExplosion);
            }
        }
    }

    // NOUVEAU : Ajout de ServerRpcParams pour identifier qui a envoyé la demande
    [ServerRpc]
    void PoserUneBombeServerRpc(Vector3 positionSurGrille, int portee, ServerRpcParams rpcParams = default)
    {
        GameObject nouvelleBombe = Instantiate(prefabBombe, positionSurGrille, Quaternion.identity);

        Bombe scriptBombe = nouvelleBombe.GetComponent<Bombe>();
        if (scriptBombe != null)
        {
            scriptBombe.porteeExplosion = portee; 
            // SUPPRIMÉ : On ne lie plus l'inventaire ici
        }

        NetworkObject netObj = nouvelleBombe.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            // CRUCIAL : Le serveur donne la propriété de la bombe au Client expéditeur
            netObj.SpawnWithOwnership(rpcParams.Receive.SenderClientId);
        }
    }

    // NOUVEAU : Le Serveur appelle cette fonction pour dire au Client de recréditer sa bombe
    [ClientRpc]
    public void RendreBombeClientRpc()
    {
        // Seul le propriétaire du personnage met à jour son inventaire local
        if (IsOwner && monInventaire != null)
        {
            // À ADAPTER : Appelez ici la fonction de votre script Inventaire qui fait l'inverse de AjouterBombeSurCarte()
            monInventaire.EnleverBombeDeCarte();
        }
    }
}