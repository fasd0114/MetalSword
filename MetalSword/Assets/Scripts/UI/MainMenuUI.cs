using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;            
public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;

    void Start()
    {
        if (startButton == null || exitButton == null)
        {
            return;
        }

        var tmp = startButton.GetComponentInChildren<TMP_Text>();
        if (tmp != null && DataManager.Instance != null && DataManager.Instance.HasSaveData)
        {
            tmp.text = "Continue";
        }
        else if (tmp == null)
        {
        }

        startButton.onClick.AddListener(OnStartClicked);
        exitButton.onClick.AddListener(OnExitClicked);
    }

    private void OnStartClicked()
    {
        SceneManager.LoadScene("GameScene");
    }

    private void OnExitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
