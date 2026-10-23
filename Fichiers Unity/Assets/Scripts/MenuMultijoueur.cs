using UnityEngine;
using UnityEngine.UIElements;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Text;
using System.Collections.Generic;

// ⚠️ NOUVEAU : On hérite de NetworkBehaviour (et plus MonoBehaviour) pour envoyer des ordres sur le réseau
public class MenuMultijoueur : NetworkBehaviour 
{
    [Header("Éléments à activer en jeu")]
    public GameObject zoneDeJeu; 
    public GameObject interfaceJeu; 

    private VisualElement root;

    // --- CONTENEURS ---
    private VisualElement conteneurPrincipal;
    private VisualElement conteneurHost;
    private VisualElement conteneurClient;

    // --- BOUTONS PRINCIPAUX ---
    private Button boutonMenuHost;
    private Button boutonMenuClient;

    // --- LOBBY HOST ---
    private Label texteCode;
    private Label listeJoueurs;
    private Button boutonDemarrer;

    // --- LOBBY CLIENT ---
    private TextField champPseudo;
    private TextField champCode;
    private Button boutonRejoindre;
private VisualElement boiteAttente;

    // Dictionnaire pour stocker les pseudos des joueurs connectés (Uniquement utilisé par le Serveur)
    private Dictionary<ulong, string> joueursConnectes = new Dictionary<ulong, string>();

    async void Start()
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // On configure le serveur pour qu'il "écoute" et lise les données de connexion (le Pseudo)
        NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprobationConnexion;
        
        // On écoute les connexions et déconnexions pour mettre à jour l'UI
        NetworkManager.Singleton.OnClientConnectedCallback += SurJoueurConnecte;
        NetworkManager.Singleton.OnClientDisconnectCallback += SurJoueurDeconnecte;
    }

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        
        // Récupération des conteneurs
        conteneurPrincipal = root.Q<VisualElement>("ConteneurPrincipal");
        conteneurHost = root.Q<VisualElement>("ConteneurHost");
        conteneurClient = root.Q<VisualElement>("ConteneurClient");

        // Récupération des éléments
        boutonMenuHost = root.Q<Button>("Host");
        boutonMenuClient = root.Q<Button>("Client");
        texteCode = root.Q<Label>("TexteCode");
        listeJoueurs = root.Q<Label>("ListeJoueurs");
        boutonDemarrer = root.Q<Button>("BoutonDemarrer");
        champPseudo = root.Q<TextField>("ChampPseudo");
        champCode = root.Q<TextField>("ChampCode");
        boutonRejoindre = root.Q<Button>("BoutonRejoindre");
        boiteAttente = root.Q<VisualElement>("Attente");

        if (boiteAttente != null) boiteAttente.style.display = DisplayStyle.None; // On le cache par sécurité au démarrage

        // Assignation des clics
        if (boutonMenuHost != null) boutonMenuHost.clicked += OuvrirLobbyHost;
        if (boutonMenuClient != null) boutonMenuClient.clicked += OuvrirLobbyClient;
        if (boutonDemarrer != null) boutonDemarrer.clicked += ClicDemarrerPartie;
        if (boutonRejoindre != null) boutonRejoindre.clicked += RejoindreRelay;
    }

    // ==========================================
    // ACTIONS DE L'HÔTE
    // ==========================================
    private async void OuvrirLobbyHost()
    {
        // On change d'écran
        conteneurPrincipal.style.display = DisplayStyle.None;
        conteneurHost.style.display = DisplayStyle.Flex;

        try {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            string codeRejoindre = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            
            if (texteCode != null) texteCode.text = "Code secret : " + codeRejoindre;
            
            RelayServerData relayServerData = allocation.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost(); 
            
            // L'hôte s'ajoute lui-même à sa propre liste
            joueursConnectes[NetworkManager.Singleton.LocalClientId] = "Hôte (Vous)";
            MettreAJourAffichageListe();
        }
        catch (System.Exception e) { Debug.LogError("Erreur Host: " + e.Message); }
    }

    private void ClicDemarrerPartie()
    {
        // Seul le serveur a le droit de donner le coup d'envoi
        if (IsServer)
        {
            // Cette commande va exécuter la fonction LancerJeuClientRpc() sur TOUS les PC connectés
            LancerJeuClientRpc();
        }
    }

    // ==========================================
    // ACTIONS DU CLIENT
    // ==========================================
    private void OuvrirLobbyClient()
    {
        conteneurPrincipal.style.display = DisplayStyle.None;
        conteneurClient.style.display = DisplayStyle.Flex;
    }

    private async void RejoindreRelay()
    {
        string pseudo = champPseudo.value;
        string code = champCode.value;

        if (string.IsNullOrEmpty(pseudo) || string.IsNullOrEmpty(code)) return;

        // On cache simplement le bouton pour éviter que le joueur clique plusieurs fois pendant le chargement
        if (boutonRejoindre != null) boutonRejoindre.style.display = DisplayStyle.None;

        try {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(code);
            RelayServerData relayServerData = joinAllocation.ToRelayServerData("dtls"); // Ou "wss" si le réseau bloque
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            // On attache le Pseudo au billet d'entrée
            NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.UTF8.GetBytes(pseudo);
            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e) { 
            Debug.LogError("Erreur Client: " + e.Message);
            // Si la connexion échoue (mauvais code), on réaffiche le bouton pour réessayer
            if (boutonRejoindre != null) boutonRejoindre.style.display = DisplayStyle.Flex;
        }
    }

    private void SurJoueurConnecte(ulong clientId)
    {
        // Si je suis le Client et que le Serveur vient de m'accepter officiellement
        if (!IsServer && clientId == NetworkManager.Singleton.LocalClientId)
        {
            // J'affiche le bel encadré noir d'attente
            if (boiteAttente != null) boiteAttente.style.display = DisplayStyle.Flex;
        }
    }

    // ==========================================
    // LOGIQUE DE SYNCHRONISATION (Réseau)
    // ==========================================

    // Fonction appelée SUR LE SERVEUR quand quelqu'un frappe à la porte
    private void ApprobationConnexion(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;
        
        // On déchiffre le Pseudo caché dans le billet d'entrée
        string pseudoClient = Encoding.UTF8.GetString(request.Payload);
        if (string.IsNullOrEmpty(pseudoClient)) pseudoClient = "Joueur Inconnu";

        // On enregistre le pseudo dans la mémoire du Serveur
        joueursConnectes[request.ClientNetworkId] = pseudoClient;
        
        // On met à jour l'écran de l'Hôte
        MettreAJourAffichageListe();
    }


    private void SurJoueurDeconnecte(ulong clientId)
    {
        // Si un joueur quitte, le Serveur l'efface de la liste
        if (IsServer && joueursConnectes.ContainsKey(clientId))
        {
            joueursConnectes.Remove(clientId);
            MettreAJourAffichageListe();
        }
    }

    private void MettreAJourAffichageListe()
    {
        if (listeJoueurs == null) return;
        
        string texteFinal = "Joueurs prêts :\n";
        foreach(var pseudo in joueursConnectes.Values)
        {
            texteFinal += "🟢 " + pseudo + "\n";
        }
        listeJoueurs.text = texteFinal;
    }

    // [ClientRpc] signifie : "Cette fonction est déclenchée par le Serveur, mais s'exécute sur TOUS les écrans en même temps"
    [ClientRpc]
    private void LancerJeuClientRpc()
    {
        // 1. On cache l'UI du Menu complètement
        if (root != null) root.style.display = DisplayStyle.None;

        // 2. On allume le générateur de carte et l'inventaire pour tout le monde !
        if (zoneDeJeu != null) zoneDeJeu.SetActive(true);
        if (interfaceJeu != null) interfaceJeu.SetActive(true);
    }
}