using UnityEngine;
using UnityEngine.UIElements;
using System.Collections;

public class MenuCarte : MonoBehaviour
{
    [Header("Le Générateur de la scène")]
    public GenerateurMap generateur;

    [Header("Les Écrans")]
    public GameObject ecranJeu; 

    [Header("Les configurations (Fiches) de tes niveaux")]
    public ConfigNiveau configNiveau1; 
    public ConfigNiveau configNiveau2; 
    public ConfigNiveau configNiveau3; 
    public ConfigNiveau configNiveau4; 
    public ConfigNiveau configNiveau5; 
    
    private VisualElement root;

    // --- LES MÉMOIRES DE PROGRESSION ---
    private int niveauEnCours = 1;
    // 'static' permet de conserver le palier atteint même quand l'écran s'éteint et se rallume !
    public static int niveauMaxDebloque = 1; 

    void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.Flex;

        Button boutonNiveau1 = root.Q<Button>("Niveau1");
        Button boutonNiveau2 = root.Q<Button>("Niveau2");
        Button boutonNiveau3 = root.Q<Button>("Niveau3");
        Button boutonNiveau4 = root.Q<Button>("Niveau4");
        Button boutonNiveau5 = root.Q<Button>("Niveau5");

        if (boutonNiveau1 != null) boutonNiveau1.clicked += () => LancerNiveau(1, configNiveau1);
        if (boutonNiveau2 != null) boutonNiveau2.clicked += () => LancerNiveau(2, configNiveau2);
        if (boutonNiveau3 != null) boutonNiveau3.clicked += () => LancerNiveau(3, configNiveau3);
        if (boutonNiveau4 != null) boutonNiveau4.clicked += () => LancerNiveau(4, configNiveau4);
        if (boutonNiveau5 != null) boutonNiveau5.clicked += () => LancerNiveau(5, configNiveau5);

        // --- NOUVEAU : À chaque réapparition de la carte, on restaure l'affichage ! ---
        ActualiserAffichageCarte();
    }

    // Fonction qui restaure l'état visuel de la carte selon ta progression
    private void ActualiserAffichageCarte()
    {
        if (root == null) return;

        for (int i = 1; i <= 5; i++)
        {
            Button btn = root.Q<Button>("Niveau" + i);
            ProgressBar chemin = root.Q<ProgressBar>("Chemin" + i);

            if (btn != null)
            {
                // Si le niveau fait partie de ceux débloqués
                if (i <= niveauMaxDebloque)
                {
                    btn.style.display = DisplayStyle.Flex;

                    // Si c'est un niveau réussi (inférieur au max actuel), on le met en vert !
                    if (i < niveauMaxDebloque)
                    {
                        btn.style.borderTopColor = new StyleColor(Color.green);
                        btn.style.borderBottomColor = new StyleColor(Color.green);
                        btn.style.borderLeftColor = new StyleColor(Color.green);
                        btn.style.borderRightColor = new StyleColor(Color.green);
                    }
                }
                else
                {
                    btn.style.display = DisplayStyle.None;
                }
            }

            // Pour les barres de progression
            if (chemin != null)
            {
                if (i < niveauMaxDebloque)
                {
                    chemin.style.display = DisplayStyle.Flex;
                    chemin.value = 100;
                }
                else
                {
                    chemin.style.display = DisplayStyle.None;
                    chemin.value = 0;
                }
            }
        }
    }

    private void LancerNiveau(int numeroNiveau, ConfigNiveau configChoisie)
    {
        Debug.Log("Je lance le niveau " + numeroNiveau + "...");
        niveauEnCours = numeroNiveau;

        if (ecranJeu != null)
        {
            ecranJeu.SetActive(true);
        }

        if (generateur != null)
        {
            generateur.LancerNouveauNiveau(configChoisie);
        }
        
        gameObject.SetActive(false); 
    }

    public void ReafficherCarte()
    {
        if (generateur != null) generateur.NettoyerAncienneMap(); 
        gameObject.SetActive(true);
    }

    public void DeclencherVictoire()
    {
        if (generateur != null) generateur.NettoyerPlateau();

        if (ecranJeu != null) ecranJeu.SetActive(false);
        gameObject.SetActive(true); 

        StartCoroutine(AnimerBarreEtDebloquer(niveauEnCours));
    }

    private IEnumerator AnimerBarreEtDebloquer(int niveauTermine)
    {
        var rootDoc = GetComponent<UIDocument>().rootVisualElement;

        // On peint le niveau terminé en vert
        Button boutonTermine = rootDoc.Q<Button>("Niveau" + niveauTermine);
        if (boutonTermine != null)
        {
            boutonTermine.style.borderTopColor = new StyleColor(Color.green);
            boutonTermine.style.borderBottomColor = new StyleColor(Color.green);
            boutonTermine.style.borderLeftColor = new StyleColor(Color.green);
            boutonTermine.style.borderRightColor = new StyleColor(Color.green);
        }

        if (niveauTermine >= 5) 
        {
            Debug.Log("Jeu terminé ! Bravo !");
            yield break;
        }

        string nomChemin = "Chemin" + niveauTermine;
        string nomNiveauSuivant = "Niveau" + (niveauTermine + 1);

        ProgressBar barreChemin = rootDoc.Q<ProgressBar>(nomChemin);
        Button boutonNiveauSuivant = rootDoc.Q<Button>(nomNiveauSuivant);

        if (barreChemin != null)
        {
            barreChemin.style.display = DisplayStyle.Flex;
            float temps = 0f;
            float duree = 3f; 

            while (temps < duree)
            {
                temps += Time.deltaTime;
                barreChemin.value = Mathf.Lerp(0, 100, temps / duree);
                yield return null; 
            }
            barreChemin.value = 100;
        }

        if (boutonNiveauSuivant != null) 
        {
            boutonNiveauSuivant.style.display = DisplayStyle.Flex;
        }

        // --- NOUVEAU : On enregistre que le niveau suivant est désormais débloqué ! ---
        if (niveauTermine + 1 > niveauMaxDebloque)
        {
            niveauMaxDebloque = niveauTermine + 1;
        }
    }
}