using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;
using System;
using Unity.Netcode;

public class Jeu : NetworkBehaviour
{
    [Header("Victoire")]
    public MenuCarte menuCarte; 
    private bool partieEnCours = false;

    [Header("Gestion des Écrans")]
    public UIDocument ecranGameOver;
    public GameObject ecranPause;

    public Inventaire joueur;
    private Label labelVitesse;
    private Label labelSac;
    private Label labelHuileRes;
    
    // Nouveaux éléments UI
    private Button boutonPatins;
    private Label labelPatinsEnJeu;

    private Button boutonSac;
    private Label labelBombesEnJeu;

    private Button boutonHuile;
    private Label labelHuileEnJeu;

    private Button boutonPause;

    void OnEnable()
    {
        if (ecranGameOver != null)
        {
            ecranGameOver.gameObject.SetActive(false);
        }
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.Flex;

        labelVitesse = root.Q<Label>("LabelVitesseRes");
        labelSac = root.Q<Label>("LabelSacRes");
        labelHuileRes = root.Q<Label>("LabelHuileRes");
        
        // On récupère le bouton et le label des patins en jeu
        boutonPatins = root.Q<Button>("Patins");
        labelPatinsEnJeu = root.Q<Label>("PatinsEnJeu");

        // On récupère le bouton et le label des bombes en jeu
        boutonSac = root.Q<Button>("Sac");
        labelBombesEnJeu = root.Q<Label>("BombesEnJeu");

        // On récupère le bouton et le label de l'huile en jeu
        boutonHuile = root.Q<Button>("Huile");
        labelHuileEnJeu = root.Q<Label>("HuileEnJeu");

        // On récupère le bouton de pause
        boutonPause = root.Q<Button>("Pause");

        // On écoute le clic sur le bouton
        if (boutonPatins != null) boutonPatins.clicked += SurClicPatins;
        if (boutonSac != null) boutonSac.clicked += SurClicSac;
        if (boutonHuile != null) boutonHuile.clicked += SurClicHuile;
        if (boutonPause != null) boutonPause.clicked += SurClicPause;

        if (joueur != null)
        {
            // Si le joueur s'était connecté pendant que l'UI dormait (ou chargeait),
            // on force la mise à jour des textes maintenant qu'elle est bien réveillée !
            MAJVitesse(joueur.reserveVitesse);
            MAJSac(joueur.reserveSac);
            MAJHuile(joueur.reserveHuile);
            
            MAJPatinsEnJeu(joueur.patinsEnJeu, joueur.maxPatins);
            MAJBombesEnJeu(joueur.bombesEnJeu, joueur.maxBombes);
            MAJHuileEnJeu(joueur.huileEnJeu, joueur.maxHuile);
        }
    }

    void OnDisable()
    {
        if (boutonPatins != null)
        {
            boutonPatins.clicked -= SurClicPatins;
        }

        if (joueur != null)
        {
            joueur.OnVitesseChanged -= MAJVitesse;
            joueur.OnSacChanged -= MAJSac;
            joueur.OnHuileChanged -= MAJHuile;
            joueur.OnPatinsEnJeuChanged -= MAJPatinsEnJeu;
            joueur.OnBombesEnJeuChanged -= MAJBombesEnJeu;
            joueur.OnHuileEnJeuChanged -= MAJHuileEnJeu;
        }
        if (boutonSac != null) boutonSac.clicked -= SurClicSac;
        if (boutonPause != null) boutonPause.clicked -= SurClicPause;
    }

    // ========================================================
    // --- NOUVEAU : GESTION DES RACCOURCIS CLAVIER ---
    // ========================================================
    void Update()
    {
        // On vérifie si la partie est en pause (Time.timeScale == 0) pour ne pas 
        // utiliser d'objets pendant que le menu pause est affiché !
        if (Time.timeScale > 0f)
        {
            if (Input.GetKeyDown(KeyCode.V)) SurClicPatins();
            if (Input.GetKeyDown(KeyCode.B)) SurClicSac();
            if (Input.GetKeyDown(KeyCode.H)) SurClicHuile();
            
            // Un petit raccourci très standard pour la pause !
            if (Input.GetKeyDown(KeyCode.Escape)) SurClicPause();
        }
    }
    // ========================================================

    // Fonction déclenchée quand on clique sur les boutons ou les touches
    private void SurClicPatins()
    {
        if (joueur != null) joueur.UtiliserPatin();
        Console.WriteLine("Patin utilisé !");
    }
    private void SurClicHuile()
    {
        if (joueur != null) joueur.UtiliserHuile();
        Console.WriteLine("Huile utilisée !");
    }
    private void SurClicSac()
    {
        if (joueur != null) joueur.UtiliserSac();
        Console.WriteLine("Sac utilisé !");
    }

    // Fonctions pour mettre à jour les labels
    private void MAJVitesse(int montant)
    {
        if (labelVitesse != null) labelVitesse.text = "Restant : " + montant;
    }

    private void MAJSac(int montant)
    {
        if (labelSac != null) labelSac.text = "Restant : " + montant;
    }

    private void MAJHuile(int montant)
    {
        if (labelHuileRes != null) labelHuileRes.text = "Restant : " + montant;
    }

    private void MAJHuileEnJeu(int enJeu, int max)
    {
        if (labelHuileEnJeu != null) labelHuileEnJeu.text = "En jeu : " + enJeu + "/" + max;
    }

    private void MAJPatinsEnJeu(int enJeu, int max)
    {
        if (labelPatinsEnJeu != null) labelPatinsEnJeu.text = "En jeu : " + enJeu + "/" + max;
    }

    private void MAJBombesEnJeu(int enJeu, int max)
    {
        if (labelBombesEnJeu != null) labelBombesEnJeu.text = "En jeu : " + enJeu + "/" + max;
    }

    // Le générateur appellera cette fonction pour connecter le joueur fraîchement créé
    public void AssignerJoueur(Inventaire nouveauJoueur)
    {
        joueur = nouveauJoueur;

        // --- NOUVEAU : On force la recherche des éléments UI immédiatement ---
        // Cela garantit que les labels ne sont pas nuls même si OnEnable a été pris de vitesse.
        var root = GetComponent<UIDocument>().rootVisualElement;
        if (root != null)
        {
            if (labelVitesse == null) labelVitesse = root.Q<Label>("LabelVitesseRes");
            if (labelSac == null) labelSac = root.Q<Label>("LabelSacRes");
            if (labelHuileRes == null) labelHuileRes = root.Q<Label>("LabelHuileRes");
            
            if (labelPatinsEnJeu == null) labelPatinsEnJeu = root.Q<Label>("PatinsEnJeu");
            if (labelBombesEnJeu == null) labelBombesEnJeu = root.Q<Label>("BombesEnJeu");
            if (labelHuileEnJeu == null) labelHuileEnJeu = root.Q<Label>("HuileEnJeu");
        }

        // On s'abonne aux événements du nouveau joueur
        joueur.OnVitesseChanged += MAJVitesse;
        joueur.OnSacChanged += MAJSac;
        joueur.OnHuileChanged += MAJHuile;
        joueur.OnPatinsEnJeuChanged += MAJPatinsEnJeu;
        joueur.OnBombesEnJeuChanged += MAJBombesEnJeu;
        joueur.OnHuileEnJeuChanged += MAJHuileEnJeu;

        Debug.Log($"Connexion UI réussie. Bombes du joueur : {joueur.reserveSac}");

        // On met à jour les textes immédiatement avec ses valeurs de départ
        MAJVitesse(joueur.reserveVitesse);
        MAJSac(joueur.reserveSac);
        MAJHuile(joueur.reserveHuile);
        MAJPatinsEnJeu(joueur.patinsEnJeu, joueur.maxPatins);
        MAJHuileEnJeu(joueur.huileEnJeu, joueur.maxHuile);
        MAJBombesEnJeu(joueur.bombesEnJeu, joueur.maxBombes);
    }

    public void AfficherGameOver()
    {
        // 1. On allume le Game Over
        if (ecranGameOver != null)
        {
            ecranGameOver.gameObject.SetActive(true);
        }
        
        // 2. On éteint l'interface de Jeu. 
        // Très important : désactiver ce GameObject arrête le script Jeu, on le fait donc en dernier !
        gameObject.SetActive(false);
    }

    private void SurClicPause()
    {
        // 1. On allume l'écran de pause (qui va s'afficher par-dessus le jeu)
        if (ecranPause != null)
        {
            ecranPause.SetActive(true);
        }

        // 2. LA MAGIE : On arrête le temps du jeu !
        Time.timeScale = 0f; 
    }

    public void DemarrerDetectionVictoire()
    {
        StartCoroutine(VerifierVictoire());
    }

    private IEnumerator VerifierVictoire()
    {
        yield return new WaitForSeconds(1f); // Laisse le générateur poser les monstres
        partieEnCours = true;

        while (partieEnCours)
        {
            int monstresRestants = GameObject.FindGameObjectsWithTag("Monstre").Length;
            
            if (monstresRestants == 0)
            {
                Debug.Log("🎉 VICTOIRE ! Lancement de l'animation...");
                partieEnCours = false; // On arrête de chercher des monstres
                
                // 1. On cherche l'Animator du joueur pour lancer sa danse
                GameObject joueurTrouve = GameObject.FindWithTag("Player"); 
                if (joueurTrouve != null)
                {
                    Animator animJoueur = joueurTrouve.GetComponent<Animator>();
                    if (animJoueur != null)
                    {
                        animJoueur.SetTrigger("Victoire");
                    }
                }

                // 2. LA PAUSE : On attend que l'animation se termine 
                yield return new WaitForSeconds(5f); 
                
                // ========================================================
                // --- SAUVEGARDE DE LA MÉMOIRE ---
                // ========================================================
                if (joueurTrouve != null)
                {
                    Inventaire inv = joueurTrouve.GetComponent<Inventaire>();
                    if (inv != null) 
                    {
                        MemoireInventaire.Sauvegarder(inv);
                        Debug.Log("💾 Inventaire sauvegardé pour le niveau suivant !");
                    }
                }
                // ========================================================

                // 3. Après l'attente, on déclenche le nettoyage et la carte
                if (menuCarte != null) 
                {
                    menuCarte.DeclencherVictoire();
                }
            }
            
            // On revérifie toutes les demi-secondes tant que la partie est en cours
            yield return new WaitForSeconds(0.5f); 
        }
    }
}