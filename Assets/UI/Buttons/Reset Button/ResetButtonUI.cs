using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component that resets the game when clicked.
/// Attach to a button to provide reset game functionality.
/// </summary>
public class ResetButtonUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameService gameService;
    [SerializeField] private Button button;

    [Header("Settings")]
    [SerializeField] private bool requireConfirmation = true;
    [SerializeField] private string confirmationMessage = "Reset game and clear all data?";

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnResetButtonClicked);
        }
    }

    private void OnResetButtonClicked()
    {
        if (gameService == null)
        {
            Debug.LogError("ResetButtonUI: GameService not assigned!");
            return;
        }

        if (requireConfirmation)
        {
            // In a real implementation, you'd show a proper UI confirmation dialog
            // For now, this just resets directly
            Debug.Log(confirmationMessage);
            ResetGame();
        }
        else
        {
            ResetGame();
        }
    }

    private void ResetGame()
    {
        Debug.Log("ResetButtonUI: Resetting game...");
        gameService.ResetGame();
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(OnResetButtonClicked);
        }
    }
}
