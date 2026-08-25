using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
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

    void OnEnable()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;

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

        // On écoute le clic sur le bouton
        if (boutonPatins != null)
        {
            boutonPatins.clicked += SurClicPatins;
        }

        if (boutonSac != null) boutonSac.clicked += SurClicSac;
        if (boutonHuile != null) boutonHuile.clicked += SurClicHuile;

        if (joueur != null)
        {
            joueur.OnVitesseChanged += MAJVitesse;
            joueur.OnSacChanged += MAJSac;
            joueur.OnHuileChanged += MAJHuile;

            joueur.OnPatinsEnJeuChanged += MAJPatinsEnJeu; // Nouvel abonnement
            joueur.OnBombesEnJeuChanged += MAJBombesEnJeu;
            joueur.OnHuileEnJeuChanged += MAJHuileEnJeu;
            
            MAJVitesse(joueur.reserveVitesse);
            MAJSac(joueur.reserveSac);
            MAJHuile(joueur.reserveHuile);
            MAJPatinsEnJeu(joueur.patinsEnJeu, joueur.maxPatins);
            MAJHuileEnJeu(joueur.huileEnJeu, joueur.maxHuile);
            MAJBombesEnJeu(joueur.bombesEnJeu, joueur.maxBombes);
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
    }

    // Fonction déclenchée quand on clique sur le bouton "Patins"
    private void SurClicPatins()
    {
        if (joueur != null)
        {
            joueur.UtiliserPatin();
        }
    }
    private void SurClicHuile()
    {
        if (joueur != null)
        {
            joueur.UtiliserHuile();
        }
    }
    private void SurClicSac()
    {
        if (joueur != null) joueur.UtiliserSac();
    }




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



    // Nouvelle fonction pour afficher "En jeu : X/Y"
    private void MAJPatinsEnJeu(int enJeu, int max)
    {
        if (labelPatinsEnJeu != null) labelPatinsEnJeu.text = "En jeu : " + enJeu + "/" + max;
    }



    private void MAJBombesEnJeu(int enJeu, int max)
    {
        if (labelBombesEnJeu != null) labelBombesEnJeu.text = "En jeu : " + enJeu + "/" + max;
    }
}