using UnityEngine;
using Unity.Netcode;

public class GenerateurDeCarte : MonoBehaviour
{
    [Header("Modèles (Prefabs)")]
    public GameObject prefabSol;
    public GameObject prefabMurIndestructible;
    public GameObject prefabMurDestructible;

    [Header("Paramètres de la Grille")]
    public int largeur = 15;
    public int profondeur = 13;
    [Range(0, 100)] public int pourcentageMursDestructibles = 70;

    [Header("Hauteurs (Y)")]
    public float hauteurSol = -1.75f;
    public float hauteurMur = -0.75f;

    void Start()
    {
        Debug.Log("⏳ Le générateur s'allume ! Attente de 0.5s...");
        Invoke("GenererNiveau", 0.5f);
    }

    void GenererNiveau()
    {
        Debug.Log("🛠️ Début de la génération. Suis-je reconnu comme Serveur ? : " + NetworkManager.Singleton.IsServer);
        int mursCrees = 0;

        for (int x = 0; x < largeur; x++)
        {
            for (int z = 0; z < profondeur; z++)
            {
                Vector3 positionSol = new Vector3(x, hauteurSol, z);
                Instantiate(prefabSol, positionSol, Quaternion.identity, transform);

                bool estSurUnBord = (x == 0 || x == largeur - 1 || z == 0 || z == profondeur - 1);
                bool estUnPilier = (x % 2 == 0 && z % 2 == 0);
                Vector3 positionMur = new Vector3(x, hauteurMur, z);

                if (estSurUnBord || estUnPilier)
                {
                    Instantiate(prefabMurIndestructible, positionMur, Quaternion.identity, transform);
                }
                else
                {
                    bool estDansUnCoin = (x <= 2 && z <= 2) || (x >= largeur - 3 && z >= profondeur - 3) || (x <= 2 && z >= profondeur - 3) || (x >= largeur - 3 && z <= 2);

                    if (!estDansUnCoin && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                    {
                        if (Random.Range(0, 100) < pourcentageMursDestructibles)
                        {
                            GameObject nouveauMur = Instantiate(prefabMurDestructible, positionMur, Quaternion.identity);
                            NetworkObject netObj = nouveauMur.GetComponent<NetworkObject>();
                            if (netObj != null)
                            {
                                netObj.Spawn();
                                mursCrees++;
                            }
                        }
                    }
                }
            }
        }
        
        Debug.Log($"✅ Génération terminée ! Murs destructibles créés en coulisses : {mursCrees}");
    }
}