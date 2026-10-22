using UnityEngine;
using UnityEngine.UIElements; 
using Unity.Netcode;          

public class MenuMultijoueur : MonoBehaviour 
{
    [Header("Éléments à activer en jeu")]
    // Pour ton GenerateurMulti
    public GameObject zoneDeJeu; 
    // --- NOUVEAU : Pour ton UI d'inventaire ---
    public GameObject interfaceJeu; 

    private Button boutonHost;
    private Button boutonClient;
    private VisualElement root; 

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        
        boutonHost = root.Q<Button>("Host");
        boutonClient = root.Q<Button>("Client");

        if (boutonHost != null) boutonHost.clicked += SurClicHost;
        if (boutonClient != null) boutonClient.clicked += SurClicClient;
    }
    
    void OnDisable()
    {
        if (boutonHost != null) boutonHost.clicked -= SurClicHost;
        if (boutonClient != null) boutonClient.clicked -= SurClicClient;
    }

    private void SurClicHost()
    {
        NetworkManager.Singleton.StartHost(); 

        // On active le générateur de carte
        if (zoneDeJeu != null) zoneDeJeu.SetActive(true);
        
        // --- NOUVEAU : On active l'interface d'inventaire ---
        if (interfaceJeu != null) interfaceJeu.SetActive(true);
        
        MasquerMenu();
    }

    private void SurClicClient()
    {
        NetworkManager.Singleton.StartClient();
        
        // On active le générateur de carte localement
        if (zoneDeJeu != null) zoneDeJeu.SetActive(true);
        
        // --- NOUVEAU : On active l'interface d'inventaire ---
        if (interfaceJeu != null) interfaceJeu.SetActive(true);
        
        MasquerMenu();
    }

    private void MasquerMenu()
    {
        if (root != null)
        {
            root.style.display = DisplayStyle.None;
        }
    }
}