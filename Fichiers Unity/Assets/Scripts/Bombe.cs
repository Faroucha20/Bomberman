using UnityEngine;
using Unity.Netcode;
using System.Collections; 

public class Bombe : NetworkBehaviour
{
    [Header("Paramètres visuels")]
    public GameObject prefabExplosion;

    [Header("Paramètres de gameplay")]
    public float tempsAvantExplosion = 3f;
    public int porteeExplosion = 1; 
    public LayerMask calqueMurs;    

    // L'ancienne variable inventaireJoueur a été supprimée

    private Collider monCollider;

    public override void OnNetworkSpawn()
    {
        monCollider = GetComponent<Collider>();
        if (monCollider != null)
        {
            monCollider.isTrigger = true; 
        }

        if (IsServer)
        {
            StartCoroutine(RoutineExplosion());
        }
    }

    private IEnumerator RoutineExplosion()
    {
        yield return new WaitForSeconds(tempsAvantExplosion);
        Exploser();
    }

    void Exploser()
    {
        // 1. On retrouve à qui appartient la bombe via le réseau
        ulong idProprietaire = GetComponent<NetworkObject>().OwnerClientId;
        
        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(idProprietaire, out NetworkClient client))
        {
            PoseurDeBombe scriptJoueur = client.PlayerObject.GetComponent<PoseurDeBombe>();
            if (scriptJoueur != null)
            {
                // Le Serveur ordonne au Client spécifique de rendre la munition
                scriptJoueur.RendreBombeClientRpc(); 
            }
        }

        // 2. On fait apparaître la flamme centrale
        Vector3 positionExplosion = transform.position + new Vector3(0, 0.5f, 0);
        CreerFlammeSurReseau(positionExplosion);

        // 3. On propage dans les 4 directions
        PropagerExplosion(Vector3.forward);
        PropagerExplosion(Vector3.back);
        PropagerExplosion(Vector3.left);
        PropagerExplosion(Vector3.right);

        // 4. Le serveur détruit la bombe pour tout le monde
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
                    mur.Detruire(); 
                    CreerFlammeSurReseau(positionExplosion);
                }
                
                break; 
            }
            else
            {
                CreerFlammeSurReseau(positionExplosion);
            }
        }
    }

    private void CreerFlammeSurReseau(Vector3 position)
    {
        if (prefabExplosion != null)
        {
            GameObject flamme = Instantiate(prefabExplosion, position, Quaternion.identity);
            
            NetworkObject netObj = flamme.GetComponent<NetworkObject>();
            if (netObj != null)
            {
                netObj.Spawn();
            }
        }
    }

    void OnTriggerExit(Collider autre)
    {
        if (autre.CompareTag("Player"))
        {
            if (monCollider != null)
            {
                monCollider.isTrigger = false;
            }
        }
    }
}