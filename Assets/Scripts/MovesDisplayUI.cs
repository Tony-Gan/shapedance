using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MovesDisplayUI : MonoBehaviour
{
    [Header("UI Prefab")]
    public GameObject moveButtonPrefab; 

    [Header("UI Container")]
    public Transform buttonContainer; 

    void Start()
    {
        SelectionManager.OnSelectionChanged += HandleSelectionChanged;
        UpdateMoveButtons(null);
    }

    void OnDestroy()
    {
        SelectionManager.OnSelectionChanged -= HandleSelectionChanged;
    }

    private void HandleSelectionChanged(DraggableItem selectedItem)
    {
        PokemonStats stats = null;
        if (selectedItem != null)
        {
            stats = selectedItem.GetComponent<PokemonStats>();
        }
        UpdateMoveButtons(stats);
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

            string moveNameForPrint = move.moveNameCN; 
            Button button = buttonGO.GetComponent<Button>();
            button.onClick.AddListener(() => {
                Debug.Log("点击了招式: " + moveNameForPrint);
            });
        }
    }
}