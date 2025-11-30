using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

// implements a singleton GameManager to manage communication 
// with persistent global data across scenes
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Services")]
    [SerializeField] private Navigation uiNavigation;
    [SerializeField] private SceneNavigation sceneNavigation;
    [SerializeField] private FadeCanvasGroup screenFade;
    [SerializeField] private GameService gameService;


    private void Awake()
    {
        if (Instance)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;

            gameService.Initialize();

            uiNavigation.Initialize();

            sceneNavigation.OnSceneNavigationRequest += () =>
            {
                uiNavigation.Reset();
                StartCoroutine(sceneNavigation.NavigateToSceneRoutine(screenFade));
            };

            sceneNavigation.OnSceneLoaded += () =>
            {
                gameService.NotifySceneLoaded();
            };

            DontDestroyOnLoad(Instance);
        }
    }

    private IEnumerator Start()
    {
        screenFade.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        yield return screenFade.FadeOut();
        sceneNavigation.Initialize();
    }
}
