using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ActionPointsDisplayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image[] actionPointImages = new Image[5];
    [SerializeField] private TextMeshProUGUI statusText;

    [Header("Colors")]
    [SerializeField] private Color activeColor = Color.red;
    [SerializeField] private Color usedColor = Color.gray;

    private PokemonStats currentSelectedPokemon;

    void Start()
    {
        SelectionManager.OnSelectionChanged += HandleSelectionChanged;
        UpdateDisplay();
    }

    void OnDestroy()
    {
        SelectionManager.OnSelectionChanged -= HandleSelectionChanged;
        if (currentSelectedPokemon != null)
        {
            currentSelectedPokemon.OnActionPointsChanged -= UpdateDisplay;
        }
    }

    private void HandleSelectionChanged(DraggableItem selectedItem)
    {
        if (currentSelectedPokemon != null)
        {
            currentSelectedPokemon.OnActionPointsChanged -= UpdateDisplay;
        }

        currentSelectedPokemon = null;
        if (selectedItem != null)
        {
            currentSelectedPokemon = selectedItem.GetComponent<PokemonStats>();
            if (currentSelectedPokemon != null)
            {
                currentSelectedPokemon.OnActionPointsChanged += UpdateDisplay;
            }
        }
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (currentSelectedPokemon == null)
        {
            // 没有选中Pokemon时，隐藏所有点和状态文本
            foreach (var img in actionPointImages)
            {
                if (img != null)
                {
                    img.enabled = false;
                }
            }
            if (statusText != null)
            {
                statusText.text = "";
            }
            return;
        }

        int current = currentSelectedPokemon.CurrentActionPoints;
        int max = currentSelectedPokemon.MaxActionPoints;
        int used = max - current;

        // 更新点的显示
        for (int i = 0; i < actionPointImages.Length; i++)
        {
            if (actionPointImages[i] != null)
            {
                if (i < max)
                {
                    actionPointImages[i].enabled = true;
                    actionPointImages[i].color = i < current ? activeColor : usedColor;
                }
                else
                {
                    actionPointImages[i].enabled = false;
                }
            }
        }

        // 更新状态文本
        if (statusText != null)
        {
            statusText.text = $"已用: {used} / 剩余: {current}";
        }
    }
}
