using UnityEngine;
using Unity.Netcode; // --- NOUVEAU : Import de la librairie réseau ---

// --- MODIFICATION : On hérite de NetworkBehaviour ---
public class Bonus : NetworkBehaviour
{
    // On crée une liste des types de bonus possibles
    public enum TypeDeBonus { Vitesse, Sac, Huile }
    
    [Header("Type de ce bonus")]
    // Cette variable va créer un menu déroulant dans l'Inspector !
    public TypeDeBonus typeBonus; 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // --- SÉCURITÉ RÉSEAU ---
            // On s'assure que c'est bien NOTRE personnage local qui marche sur le bonus
            NetworkObject joueurNetObj = other.GetComponent<NetworkObject>();
            
            if (joueurNetObj != null && joueurNetObj.IsOwner)
            {
                Inventaire inventaire = other.GetComponent<Inventaire>();
                if (inventaire != null)
                {
                    // On vérifie quel est le type sélectionné dans le menu déroulant
                    switch (typeBonus)
                    {
                        case TypeDeBonus.Vitesse:
                            inventaire.AjouterBonusVitesse();
                            break;
                        
                        case TypeDeBonus.Sac:
                            inventaire.AjouterBonusSac();
                            break;
                        
                        case TypeDeBonus.Huile:
                            inventaire.AjouterBonusHuile();
                            break;
                    }

                    // --- MODIFICATION : On demande au serveur de le détruire ---
                    DemanderDestructionServerRpc();
                }
            }
        }
    }

    // --- NOUVELLE FONCTION ---
    // (RequireOwnership = false) donne l'autorisation aux clients de déclencher cette requête
    [ServerRpc(RequireOwnership = false)]
    private void DemanderDestructionServerRpc()
    {
        // Le serveur exécute le Despawn pour faire disparaître l'objet chez tous les joueurs
        if (NetworkObject != null && NetworkObject.IsSpawned)
        {
            NetworkObject.Despawn();
        }
    }
}