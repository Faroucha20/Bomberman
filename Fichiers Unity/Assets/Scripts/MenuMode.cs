using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;

public class MenuMode : MonoBehaviour
{
    [Header("Écrans Suivants")]
    public GameObject ecranMap;
    public GameObject ecranCommande;
    // --- NOUVEAU : On ajoute l'écran multijoueur ---
    public GameObject ecranMultijoueur;


    private Button boutonSolo;
    private Button boutonCommande;
    private Button boutonMultijoueur;


    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        
        // ⚠️ Vérifie que ces noms correspondent exactement à ton UI Builder !
        boutonSolo = root.Q<Button>("Solo");
        boutonCommande = root.Q<Button>("Commandes");
        // --- NOUVEAU : On connecte le bouton UI ---
        boutonMultijoueur = root.Q<Button>("Multijoueur");


        if (boutonSolo != null) boutonSolo.clicked += SurClicSolo;
        if (boutonCommande != null) boutonCommande.clicked += SurClicCommande;
        // --- NOUVEAU : On écoute le clic ---
        if (boutonMultijoueur != null) boutonMultijoueur.clicked += SurClicMultijoueur;
    }

    
    void OnDisable()
    {
        if (boutonSolo != null) boutonSolo.clicked -= SurClicSolo;
        if (boutonCommande != null) boutonCommande.clicked -= SurClicCommande;
        // --- NOUVEAU : On arrête d'écouter le clic ---
        if (boutonMultijoueur != null) boutonMultijoueur.clicked -= SurClicMultijoueur;
    }

    private void SurClicSolo()
    {
        // 1. On allume D'ABORD le GameObject de la Map et de l'interface
        if (ecranMap != null)
        {
            ecranMap.SetActive(true);
        }

        // 2. On démarre ensuite la partie (le Bomberman va apparaître et trouvera l'interface allumée)
        if (Unity.Netcode.NetworkManager.Singleton != null)
        {
            Unity.Netcode.NetworkManager.Singleton.StartHost();
        }

        // 3. On éteint l'écran d'accueil
        gameObject.SetActive(false);
    }

    private void SurClicCommande()
    {
        // On affiche la page de Commande
        if (ecranCommande != null)
        {
            ecranCommande.SetActive(true);
        }
    }

    // --- NOUVELLE FONCTION ---
    private void SurClicMultijoueur()
    {
        // 1. On allume le menu Multijoueur
        if (ecranMultijoueur != null)
        {
            ecranMultijoueur.SetActive(true);
        }

        // 2. On éteint le menu actuel (MenuMode)
        gameObject.SetActive(false);
    }
}