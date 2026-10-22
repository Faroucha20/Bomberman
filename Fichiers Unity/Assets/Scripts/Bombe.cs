using UnityEngine;
using Unity.Netcode;
using System.Collections; // NOUVEAU : Requis pour les Coroutines

public class Bombe : NetworkBehaviour
{
    [Header("Paramètres visuels")]
    public GameObject prefabExplosion;

    [Header("Paramètres de gameplay")]
    public float tempsAvantExplosion = 3f;
    public int porteeExplosion = 1; // La portée (sera modifiée par celui qui la pose !)
    public LayerMask calqueMurs;    

    [HideInInspector] public Inventaire inventaireJoueur;

    private Collider monCollider;

    // --- MODIFICATION : On utilise OnNetworkSpawn au lieu de Start ---
    public override void OnNetworkSpawn()
    {
        monCollider = GetComponent<Collider>();
        if (monCollider != null)
        {
            monCollider.isTrigger = true; 
        }

        // SÉCURITÉ RÉSEAU : Seul le Serveur a le droit de lancer le compte à rebours !
        if (IsServer)
        {
            StartCoroutine(RoutineExplosion());
        }
    }

    // Le minuteur du serveur
    private IEnumerator RoutineExplosion()
    {
        yield return new WaitForSeconds(tempsAvantExplosion);
        Exploser();
    }

    void Exploser()
    {
        // On rend la bombe à l'inventaire du joueur
        if (inventaireJoueur != null)
        {
            inventaireJoueur.EnleverBombeDeCarte();
        }

        // 1. On fait apparaître la flamme centrale sur le réseau
        Vector3 positionExplosion = transform.position + new Vector3(0, 0.5f, 0);
        CreerFlammeSurReseau(positionExplosion);

        // 2. On propage dans les 4 directions
        PropagerExplosion(Vector3.forward);
        PropagerExplosion(Vector3.back);
        PropagerExplosion(Vector3.left);
        PropagerExplosion(Vector3.right);

        // 3. Le serveur détruit la bombe pour tout le monde
        GetComponent<NetworkObject>().Despawn();
    }

    void PropagerExplosion(Vector3 direction)
    {
        for (int i = 1; i <= porteeExplosion; i++)
        {
            Vector3 positionAVerifier = transform.position + (direction * i);
            Vector3 positionExplosion = positionAVerifier + new Vector3(0, 0.5f, 0);

            Collider[] objetsTouches = Physics.OverlapSphere(positionAVerifier, 0.4f, calqueMurs);

            if (objetsTouches.Length > 0)
            {
                Collider objet = objetsTouches[0];
                MurDestructible mur = objet.GetComponent<MurDestructible>();
                
                if (mur != null)
                {
                    // Le mur gère sa propre destruction (et le largage de bonus)
                    mur.Detruire(); 
                    CreerFlammeSurReseau(positionExplosion);
                }
                
                break; // On arrête l'explosion dans cette direction car on a touché un mur
            }
            else
            {
                // La case est vide, on pose une flamme
                CreerFlammeSurReseau(positionExplosion);
            }
        }
    }

    // --- NOUVELLE FONCTION : Apparition réseau des flammes ---
    private void CreerFlammeSurReseau(Vector3 position)
    {
        if (prefabExplosion != null)
        {
            // On crée l'objet
            GameObject flamme = Instantiate(prefabExplosion, position, Quaternion.identity);
            
            // On l'annonce au réseau
            NetworkObject netObj = flamme.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
        }
    }

    void OnTriggerExit(Collider autre)
    {
        // Cette partie peut rester locale : c'est juste de la physique pour empêcher 
        // le joueur de traverser la bombe après en être sorti.
        if (autre.CompareTag("Player"))
        {
            if (monCollider != null)
            {
                monCollider.isTrigger = false;
            }
        }
    }
}