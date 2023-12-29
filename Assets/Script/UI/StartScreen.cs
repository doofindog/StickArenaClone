using TMPro;
using UnityEngine;

public class StartScreen : MonoBehaviour
{
    [SerializeField] private TMP_InputField _usernameField;

    public void OnHostPressed()
    {
        ConnectionManager.TryStartHost();
    }

    public void OnJoinPressed()
    {
        ConnectionManager.TryJoin();
    }
}
