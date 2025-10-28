using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovesDisplayUI : MonoBehaviour
{
    [Header("UI Prefab")]
    public GameObject moveButtonPrefab; 

    [Header("UI Container")]
    public Transform buttonContainer; 

    private PokemonStats currentSelectedPokemon;

    void Start()
    {
        SelectionManager.OnSelectionChanged += HandleSelectionChanged;
        UpdateMoveButtons(null);
    }

    void OnDestroy()
    {
        SelectionManager.OnSelectionChanged -= HandleSelectionChanged;
        if (currentSelectedPokemon != null)
        {
            currentSelectedPokemon.OnActionPointsChanged -= OnActionPointsChanged;
        }
    }

    private void HandleSelectionChanged(DraggableItem selectedItem)
    {
        if (currentSelectedPokemon != null)
        {
            currentSelectedPokemon.OnActionPointsChanged -= OnActionPointsChanged;
        }

        currentSelectedPokemon = null;
        if (selectedItem != null)
        {
            currentSelectedPokemon = selectedItem.GetComponent<PokemonStats>();
            if (currentSelectedPokemon != null)
            {
                currentSelectedPokemon.OnActionPointsChanged += OnActionPointsChanged;
            }
        }
        UpdateMoveButtons(currentSelectedPokemon);
    }

    private void OnActionPointsChanged()
    {
        UpdateButtonStates();
    }

    private void UpdateMoveButtons(PokemonStats pokemon)
    {
        foreach (Transform child in buttonContainer)
        {
            Destroy(child.gameObject);
        }

        if (pokemon == null)
        {
            return;
        }

        MoveBaseSO[] moves = pokemon.GetMoves();

        foreach (MoveBaseSO move in moves)
        {
            if (move == null)
            {
                continue;
            }

            GameObject buttonGO = Instantiate(moveButtonPrefab, buttonContainer);
            TextMeshProUGUI buttonText = buttonGO.GetComponentInChildren<TextMeshProUGUI>(); 

            if (buttonText != null)
            {
                buttonText.text = move.moveNameCN;
            }

            MoveBaseSO currentMove = move;
            
            Button button = buttonGO.GetComponent<Button>();
            button.onClick.AddListener(() => {
                if (TargetingManager.Instance != null && currentSelectedPokemon != null)
                {
                    TargetingManager.Instance.StartTargeting(currentSelectedPokemon, currentMove);
                }
                else
                {
                    Debug.LogWarning("TargetingManager not found or no pokemon selected.");
                }
            });
        }

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (currentSelectedPokemon == null)
        {
            return;
        }

        bool hasActionPoints = currentSelectedPokemon.CanUseActionPoint(1);

        foreach (Transform child in buttonContainer)
        {
            Button button = child.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = hasActionPoints;
            }
        }
    }
}