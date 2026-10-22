using UnityEngine;
using Unity.Netcode; // --- NOUVEAU : Importation de la librairie réseau ---

// --- MODIFICATION : On hérite de NetworkBehaviour ---
public class MurDestructible : NetworkBehaviour
{
    [Header("Paramètres des Bonus")]
    // Le tableau qui contiendra tes modèles 3D de bonus
    public GameObject[] bonusDisponibles; 
    
    // La chance de faire apparaître un bonus (sur 100)
    [Range(0, 100)]
    public int chanceDeDrop = 40;

    public void Detruire()
    {
        // --- SÉCURITÉ RÉSEAU ---
        // Seul le serveur a le droit de choisir s'il y a un bonus et de casser le mur !
        if (!IsServer) return;

        // 1. On tire un nombre au hasard entre 0 et 99
        int tirage = Random.Range(0, 100);

        // 2. On vérifie si le tirage est inférieur à notre chance de drop
        if (tirage < chanceDeDrop)
        {
            // Sécurité : on vérifie que tu as bien mis des bonus dans l'Inspector
            if (bonusDisponibles.Length > 0)
            {
                // 3. On choisit un bonus au hasard dans notre liste
                int indexAleatoire = Random.Range(0, bonusDisponibles.Length);
                GameObject bonusChoisi = bonusDisponibles[indexAleatoire];

                Vector3 spawnPosition = transform.position;
                Quaternion spawnRotation = Quaternion.Euler(-90f, -0.75f, 0f);

                // --- MODIFICATION : Instanciation Réseau ---
                // On crée le bonus localement sur le serveur...
                GameObject nouveauBonus = Instantiate(bonusChoisi, spawnPosition, spawnRotation);
                
                // ...puis on l'annonce à tous les clients pour qu'il apparaisse chez eux
                NetworkObject netObj = nouveauBonus.GetComponent<NetworkObject>();
                if (netObj != null)
                {
                    netObj.Spawn();
                }
                else
                {
                    Debug.LogWarning($"Attention, le bonus {bonusChoisi.name} n'a pas de composant NetworkObject !");
                }
            }
        }

        // --- MODIFICATION : Destruction Réseau ---
        // Le serveur ordonne la destruction du mur pour tout le monde
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
        else
        {
            // Sécurité de secours au cas où le mur ne serait pas connecté
            Destroy(gameObject);
        }
    }
}