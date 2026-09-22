using UnityEngine;

public class EffetExplosion : MonoBehaviour
{
    [Header("Paramètres de temps")]
    [Tooltip("Temps avant que l'image de l'explosion ne disparaisse")]
    public float tempsAffichage = 0.5f;
    
    [Tooltip("Temps pendant lequel l'explosion est mortelle (doit être très court)")]
    public float tempsLethal = 0.1f;

    void Start()
    {
        // 1. On désactive le Collider très vite pour ne tuer que ce qui est touché sur le coup
        Invoke("DesactiverDegats", tempsLethal);

        // 2. On détruit l'objet visuel plus tard pour laisser l'animation se terminer
        Destroy(gameObject, tempsAffichage);
    }

    // --- NOUVELLE FONCTION : C'est ici qu'on tue les monstres ! ---
    void OnTriggerEnter(Collider autre)
    {
        // On vérifie si l'objet touché possède le tag "Monstre"
        if (autre.CompareTag("Monstre"))
        {
            // Boum ! On détruit le monstre
            Destroy(autre.gameObject);
        }
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