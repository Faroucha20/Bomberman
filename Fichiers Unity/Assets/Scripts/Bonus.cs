using UnityEngine;

public class Bonus : MonoBehaviour
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
                        inventaire.AjouterBonusHuile(); // <-- La nouvelle fonction !
                        break;
                }

                Destroy(gameObject); // Fait disparaître le bonus de la carte
            }
        }
    }
}