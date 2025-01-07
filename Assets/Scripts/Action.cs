using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Action : MonoBehaviour
{
    // Agregar esto en el Start() o Awake() del script Action
    private void Start()
    {
        // Encontrar el GameStateManager y configurar las referencias
        GameStateManager gameManager = Object.FindFirstObjectByType<GameStateManager>();
        if (gameManager != null)
        {
            gameManager.SetupReferences(AreaDeck, BoardAreas);
        }
    }
    [SerializeField]
    private Button playButton;
    [SerializeField]
    private Button dealButton;
    [SerializeField]
    private Button nextButton;
    public GameObject BackPanel;
    public bool isPlayed = true;
    public GameObject CardPrefab;
    public GameObject AreaDeck;
    public List<Image> BoardAreas;
    private int lastMovedAreaIndex = -1;

    // Agregar eventos para notificar victoria o derrota
    public delegate void GameStateChanged(bool isVictory);
    public static event GameStateChanged OnGameStateChanged;

    public class Carta
    {
        public string Suit { get; private set; }
        public string Rank { get; private set; }
        public Sprite CardSprite { get; private set; }
        public bool HasFlag { get; set; } // Nueva propiedad para la bandera

        public Carta(string suit, string rank, Sprite sprite)
        {
            Suit = suit;
            Rank = rank;
            CardSprite = sprite;
            HasFlag = false; // Inicialmente sin bandera
        }

        public int GetAreaIndex()
        {
            switch (Rank)
            {
                case "A": return 0;
                case "J": return 10;
                case "Q": return 11;
                case "K": return 12;
                default:
                    if (int.TryParse(Rank, out int numericRank))
                        return numericRank - 1;
                    return -1;
            }
        }

        public override string ToString()
        {
            return $"{Rank} de {Suit}";
        }
    }

    private List<Carta> deck;

    private readonly Dictionary<string, string> suitMapping = new Dictionary<string, string>
    {
        {"Corazon", "Hearts"},
        {"Diamante", "Diamonds"},
        {"Trebole", "Clubs"},
        {"Pica", "Spades"}
    };

    private string CleanSpriteName(string spriteName)
    {
        int underscoreIndex = spriteName.IndexOf('_');
        if (underscoreIndex >= 0)
        {
            spriteName = spriteName.Substring(0, underscoreIndex);
        }
        return spriteName.ToUpper();
    }

    // Método para verificar el estado del juego
    private void CheckGameState()
    {
        bool anyAreaComplete = false;
        bool allCardsHaveFlags = true;

        foreach (Image area in BoardAreas)
        {
            if (area.transform.childCount == 0) continue;

            bool allFlagsInArea = true;
            bool anyCardInArea = false;

            foreach (Transform cardTransform in area.transform)
            {
                anyCardInArea = true;
                CardFlag cardFlag = cardTransform.GetComponent<CardFlag>();
                if (cardFlag == null || !cardFlag.HasFlag)
                {
                    allFlagsInArea = false;
                    allCardsHaveFlags = false;
                }
            }

            if (anyCardInArea && allFlagsInArea)
            {
                anyAreaComplete = true;
            }
        }

        // Condición de victoria: todas las cartas tienen bandera
        if (allCardsHaveFlags)
        {
            OnGameStateChanged?.Invoke(true);
            Debug.Log("¡Victoria! Todas las cartas han sido correctamente colocadas.");
        }
        // Condición de derrota: un área está completa pero quedan cartas sin bandera
        else if (anyAreaComplete)
        {
            OnGameStateChanged?.Invoke(false);
            Debug.Log("¡Derrota! Hay un área completa pero quedan cartas sin colocar correctamente.");
        }
    }

    public void Next()
    {
        Image areaK = BoardAreas[BoardAreas.Count - 1];

        if (areaK.transform.childCount == 0 && lastMovedAreaIndex == -1)
        {
            Debug.Log("No hay cartas en el área K y no hay cartas movidas previamente.");
            return;
        }

        Image currentArea = lastMovedAreaIndex == -1 ? areaK : BoardAreas[lastMovedAreaIndex];

        if (currentArea.transform.childCount == 0)
        {
            Debug.Log("El área actual no tiene cartas para mover.");
            return;
        }

        Transform lastCard = currentArea.transform.GetChild(currentArea.transform.childCount - 1);
        Image cardImage = lastCard.GetComponent<Image>();

        if (cardImage == null || cardImage.sprite == null)
        {
            Debug.LogError("Carta sin Sprite");
            return;
        }

        string cardRank = CleanSpriteName(cardImage.sprite.name);
        Debug.Log($"Valor de la carta levantada: {cardRank}");

        // Obtener el índice del área actual
        int currentAreaIndex = BoardAreas.IndexOf(currentArea);
        string backAreaName = "BackArea" + (currentAreaIndex + 1);

        // Si es el área K (última área), usar BackAreaK
        if (currentAreaIndex == BoardAreas.Count - 1)
        {
            backAreaName = "BackAreaK";
        }

        // Buscar y desactivar el área de fondo específica
        Transform backArea = BackPanel.transform.Find(backAreaName);
        if (backArea != null)
        {
            backArea.gameObject.SetActive(false);
            Debug.Log($"Desactivando {backAreaName} en el área {currentAreaIndex + 1}");
        }
        else
        {
            Debug.LogWarning($"No se encontró el área de fondo: {backAreaName}");
        }

        Carta cartaActual = new Carta("", cardRank, cardImage.sprite);
        int targetAreaIndex = cartaActual.GetAreaIndex();

        if (targetAreaIndex < 0 || targetAreaIndex >= BoardAreas.Count)
        {
            Debug.LogError($"Índice de área inválido para la carta {cardRank}: {targetAreaIndex}");
            return;
        }

        // Mover la carta y establecer la bandera
        if (cardRank == "K")
        {
            lastCard.SetParent(areaK.transform, false);
            lastCard.SetSiblingIndex(0);
            lastMovedAreaIndex = BoardAreas.Count - 1;

            // Establecer la bandera para la carta K
            SetCardFlag(lastCard.gameObject, true);
            Debug.Log("Carta movida al área K con bandera.");
        }
        else
        {
            lastCard.SetParent(BoardAreas[targetAreaIndex].transform, false);
            lastCard.SetSiblingIndex(0);
            lastMovedAreaIndex = targetAreaIndex;

            // Establecer la bandera para la carta en su área correcta
            SetCardFlag(lastCard.gameObject, true);
            Debug.Log($"Carta movida al área {targetAreaIndex + 1} con bandera.");
        }

        // Verificar el estado del juego después de cada movimiento
        CheckGameState();
    }

    // Método auxiliar para establecer la bandera en una carta
    private void SetCardFlag(GameObject cardObject, bool value)
    {
        CardFlag cardFlag = cardObject.GetComponent<CardFlag>();
        if (cardFlag == null)
        {
            cardFlag = cardObject.AddComponent<CardFlag>();
        }
        cardFlag.HasFlag = value;
    }

    public void Play()
    {
        if (isPlayed)
        {
            CreateDeck();
            ShuffleDeck();
            isPlayed = false;
            lastMovedAreaIndex = -1;

            foreach (Carta card in deck)
            {
                GameObject cardObject = Instantiate(CardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                cardObject.transform.SetParent(AreaDeck.transform, false);

                Image cardImage = cardObject.GetComponent<Image>();
                if (cardImage != null && card.CardSprite != null)
                {
                    cardImage.sprite = card.CardSprite;
                }

                // Asegurarse de que la carta tenga el componente CardFlag
                if (cardObject.GetComponent<CardFlag>() == null)
                {
                    cardObject.AddComponent<CardFlag>();
                }
            }
            if (playButton != null)
            {
                playButton.gameObject.SetActive(false);
            }
        }
        dealButton.gameObject.transform.position = new Vector3(dealButton.gameObject.transform.position.x, dealButton.gameObject.transform.position.y + 1, dealButton.gameObject.transform.position.z);
        nextButton.gameObject.transform.position = new Vector3(nextButton.gameObject.transform.position.x, nextButton.gameObject.transform.position.y + 1, nextButton.gameObject.transform.position.z);
    }

    public void Deal()
    {
        BackPanel.SetActive(true);
        foreach (Transform child in AreaDeck.transform)
        {
            Destroy(child.gameObject);
        }

        lastMovedAreaIndex = -1;
        int areaIndex = 0;

        foreach (Carta card in deck)
        {
            GameObject cardObject = Instantiate(CardPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            cardObject.transform.SetParent(BoardAreas[areaIndex].transform, false);

            Image cardImage = cardObject.GetComponent<Image>();
            if (cardImage != null && card.CardSprite != null)
            {
                cardImage.sprite = card.CardSprite;
            }

            // Asegurarse de que la carta tenga el componente CardFlag
            if (cardObject.GetComponent<CardFlag>() == null)
            {
                cardObject.AddComponent<CardFlag>();
            }

            areaIndex = (areaIndex + 1) % BoardAreas.Count;
        }
        deck.Clear();
        dealButton.gameObject.SetActive(false);
        nextButton.gameObject.transform.position = new Vector3(nextButton.gameObject.transform.position.x, nextButton.gameObject.transform.position.y + 1, nextButton.gameObject.transform.position.z);
    }

    private void CreateDeck()
    {
        deck = new List<Carta>();
        string[] suits = { "Corazon", "Diamante", "Trebole", "Pica" };
        string[] ranks = { "A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K" };

        foreach (string suit in suits)
        {
            foreach (string rank in ranks)
            {
                string spritePath = $"Sprites/{suitMapping[suit]}/{rank}";
                Sprite cardSprite = Resources.Load<Sprite>(spritePath);

                if (cardSprite == null)
                {
                    Debug.LogError($"No se pudo cargar el sprite para {rank} de {suit} en la ruta: {spritePath}");
                }

                deck.Add(new Carta(suit, rank, cardSprite));
            }
        }
    }

    private void ShuffleDeck()
    {
        for (int shuffleCount = 0; shuffleCount < 3; shuffleCount++) // Repetir el proceso 3 veces
        {
            // Dividir el mazo en dos mitades
            int halfSize = deck.Count / 2;
            List<Carta> leftHalf = deck.GetRange(0, halfSize);
            List<Carta> rightHalf = deck.GetRange(halfSize, deck.Count - halfSize);
            List<Carta> shuffledDeck = new List<Carta>();

            // Mezclar las mitades
            while (leftHalf.Count > 0 && rightHalf.Count > 0)
            {
                // Decidir aleatoriamente si tomar 1 o 2 cartas
                int cardsToTake = Random.Range(1, 3);

                // Tomar cartas de la mitad izquierda
                for (int i = 0; i < cardsToTake && leftHalf.Count > 0; i++)
                {
                    shuffledDeck.Add(leftHalf[0]);
                    leftHalf.RemoveAt(0);
                }

                // Tomar cartas de la mitad derecha
                cardsToTake = Random.Range(1, 3);
                for (int i = 0; i < cardsToTake && rightHalf.Count > 0; i++)
                {
                    shuffledDeck.Add(rightHalf[0]);
                    rightHalf.RemoveAt(0);
                }
            }

            // Agregar las cartas restantes
            shuffledDeck.AddRange(leftHalf);
            shuffledDeck.AddRange(rightHalf);

            // Actualizar el mazo
            deck = shuffledDeck;
        }
    }
}

// Clase auxiliar para mantener el estado de la bandera en cada carta
public class CardFlag : MonoBehaviour
{
    public bool HasFlag { get; set; } = false;
}