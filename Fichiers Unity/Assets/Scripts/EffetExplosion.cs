using UnityEngine;
using Unity.Netcode;
using System.Collections; // Requis pour utiliser les Coroutines

public class EffetExplosion : NetworkBehaviour
{
    [Header("Paramètres de temps")]
    [Tooltip("Temps avant que l'image de l'explosion ne disparaisse")]
    public float tempsAffichage = 0.5f;
    
    [Tooltip("Temps pendant lequel l'explosion est mortelle (doit être très court)")]
    public float tempsLethal = 0.1f;

    // --- MODIFICATION : On remplace Start par OnNetworkSpawn ---
    public override void OnNetworkSpawn()
    {
        // 1. TOUT LE MONDE (Serveur et Clients) lance le chrono pour désactiver le collider localement.
        // Cela empêche les bugs physiques sur les écrans des clients.
        StartCoroutine(RoutineLethale());

        // 2. SEUL LE SERVEUR lance le chrono pour détruire l'objet sur le réseau.
        if (IsServer)
        {
            StartCoroutine(RoutineDisparition());
        }
    }

    private IEnumerator RoutineLethale()
    {
        yield return new WaitForSeconds(tempsLethal);
        DesactiverDegats();
    }

    private IEnumerator RoutineDisparition()
    {
        yield return new WaitForSeconds(tempsAffichage);
        
        // Le serveur détruit l'objet pour tout le monde (Despawn)
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }

    // --- MODIFICATION : Gestion réseau des morts ---
    void OnTriggerEnter(Collider autre)
    {
        // SÉCURITÉ RÉSEAU : Seul le serveur a le droit de décider qui meurt !
        if (!IsServer) return;

        // On vérifie si l'objet touché possède le tag "Monstre"
        if (autre.CompareTag("Monstre"))
        {
            NetworkObject netObj = autre.GetComponent<NetworkObject>();
            
            // Si le monstre est sur le réseau (Multijoueur)
            if (netObj != null && netObj.IsSpawned)
            {
                netObj.Despawn();
            }
            // Si le monstre n'est pas sur le réseau (Sécurité Mode Solo)
            else
            {
                Destroy(autre.gameObject);
            }
        }
        
        // /!\ PLUS TARD : Nous ajouterons ici la logique pour tuer un joueur s'il est touché !
    }

    void DesactiverDegats()
    {
        // On cherche le Collider attaché à cette explosion
        Collider monCollider = GetComponent<Collider>();
        
        // Si on le trouve, on le désactive pour qu'il ne tue plus personne
        if (monCollider != null)
        {
            monCollider.enabled = false;
        }
    }
}