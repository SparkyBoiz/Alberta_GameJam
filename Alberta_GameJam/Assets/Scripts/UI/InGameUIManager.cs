using Game.Core;
using Game.Player;
using UnityEngine;
using UnityEngine.InputSystem;

public class InGameUIManager : Singleton<InGameUIManager>
{
    [SerializeField] GameObject pauseMenu;
    [SerializeField] GameObject gameoverMenu;

    InputSystem_Actions _inputActions;
    bool _isPaused;
    bool _restorePlayerInput;

    public override void Awake()
    {
        base.Awake();
        _inputActions = new InputSystem_Actions();
        ApplyPauseState(false, true);
    }

    void OnEnable()
    {
        _inputActions.UI.Enable();
        _inputActions.UI.Pause.performed += OnPausePerformed;
    }

    void OnDisable()
    {
        _inputActions.UI.Pause.performed -= OnPausePerformed;
        _inputActions.UI.Disable();
        ApplyPauseState(false, true);
    }

    public override void OnDestroy()
    {
        _inputActions.Disable();
    }

    void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (!context.performed)
        {
            return;
        }

        if (gameoverMenu != null && gameoverMenu.activeSelf)
        {
            return;
        }

        if (_isPaused)
        {
            Resume();
        }
        else
        {
            ShowPause();
        }
    }

    void ApplyPauseState(bool paused, bool force = false, bool updatePauseMenu = true)
    {
        if (!force && _isPaused == paused)
        {
            if (updatePauseMenu && pauseMenu != null)
            {
                pauseMenu.SetActive(paused);
            }
            return;
        }

        _isPaused = paused;

        if (pauseMenu != null && updatePauseMenu)
        {
            pauseMenu.SetActive(paused);
        }

        Time.timeScale = paused ? 0f : 1f;

        var player = TopDownPlayerController.Instance;
        if (player != null && player.inputActions != null)
        {
            var moveAction = player.inputActions.Player.Move;
            if (paused)
            {
                _restorePlayerInput = moveAction != null && moveAction.enabled;
                if (_restorePlayerInput)
                {
                    player.inputActions.Player.Disable();
                }
            }
            else
            {
                if (_restorePlayerInput)
                {
                    player.inputActions.Player.Enable();
                }
                _restorePlayerInput = false;
            }
        }
        else if (!paused)
        {
            _restorePlayerInput = false;
        }
    }

    public void ShowPause()
    {
        ApplyPauseState(true);
    }

    public void HidePause()
    {
        ApplyPauseState(false);
    }

    public void ShowGameover()
    {
        ApplyPauseState(true, false, false);

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        if (gameoverMenu != null)
        {
            gameoverMenu.SetActive(true);
        }
    }

    public void HideGameover()
    {
        if (gameoverMenu != null)
        {
            gameoverMenu.SetActive(false);
        }

        ApplyPauseState(false);
    }

    public void Resume()
    {
        HidePause();
    }

    public void BackToTitle()
    {
        ApplyPauseState(false, true);

        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        if (gameoverMenu != null)
        {
            gameoverMenu.SetActive(false);
        }

        GameManager.Instance.BackToTitle();
    }
}
