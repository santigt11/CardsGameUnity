using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic; // Agregamos esta línea para poder usar List<>

public class GameStateManager : MonoBehaviour
{
    [SerializeField] 
    private Button playButton;
    [SerializeField] 
    private Button dealButton;
    [SerializeField]
    private Button nextButton;
    public GameObject BackPanel;

    [Header("UI References")]
    public GameObject victoryPanel;
    public GameObject defeatPanel;

    [Header("Image References")]
    [Tooltip("Imagen que se mostrará cuando el jugador gane")]
    public Image victoryImage;
    [Tooltip("Imagen que se mostrará cuando el jugador pierda")]
    public Image defeatImage;

    [Header("Optional Sprite References")]
    public Sprite victorySprite;
    public Sprite defeatSprite;

    // Referencia al área del mazo inicial
    public GameObject AreaDeck;
    // Referencia a todas las áreas del tablero
    public List<Image> BoardAreas;

    private void OnEnable()
    {
        Action.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        Action.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(bool isVictory)
    {
        if (isVictory)
        {
            ShowVictory();
        }
        else
        {
            ShowDefeat();
        }
    }

    public void ShowPlayButton()
    {
        if (playButton != null)
        {
            playButton.gameObject.SetActive(true);
        }
    }

    private void ShowVictory()
    {
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);

            if (victoryImage != null && victorySprite != null)
            {
                victoryImage.sprite = victorySprite;
            }
        }

        Debug.Log("Victoria: Todas las cartas están correctamente colocadas");
    }

    private void ShowDefeat()
    {
        if (defeatPanel != null)
        {
            defeatPanel.SetActive(true);

            if (defeatImage != null && defeatSprite != null)
            {
                defeatImage.sprite = defeatSprite;
            }
        }

        Debug.Log("Derrota: Hay un grupo completo pero quedan cartas sin colocar correctamente");
    }

    public void RestartGame()
    {
        
        dealButton.gameObject.transform.position = new Vector3(dealButton.gameObject.transform.position.x, dealButton.gameObject.transform.position.y - 1, dealButton.gameObject.transform.position.z);
        nextButton.gameObject.transform.position = new Vector3(nextButton.gameObject.transform.position.x, nextButton.gameObject.transform.position.y - 2, nextButton.gameObject.transform.position.z);
        playButton.gameObject.SetActive(true);
        dealButton.gameObject.SetActive(true);
        // Ocultar paneles de victoria/derrota
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (BackPanel != null) BackPanel.SetActive(false);
        
        // Obtener referencia al script Action
        Action gameAction = Object.FindFirstObjectByType<Action>();
        if (gameAction == null)
        {
            Debug.LogError("No se encontró el componente Action!");
            return;
        }

        // Activar isPlayed
        gameAction.isPlayed = true;

        // Limpiar todas las áreas del tablero
        if (BoardAreas != null)
        {
            foreach (Image area in BoardAreas)
            {
                if (area != null)
                {
                    foreach (Transform child in area.transform)
                    {
                        Destroy(child.gameObject);
                    }
                }
            }
        }

        // Limpiar el área del mazo
        if (AreaDeck != null)
        {
            foreach (Transform child in AreaDeck.transform)
            {
                Destroy(child.gameObject);
            }
        }

        Debug.Log("Juego reiniciado completamente");
    }

    // Método para configurar las referencias desde el script Action
    public void SetupReferences(GameObject areaDeck, List<Image> boardAreas)
    {
        AreaDeck = areaDeck;
        BoardAreas = boardAreas;
    }
}