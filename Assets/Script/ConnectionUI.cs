using TMPro;
using UnityEngine;

public class ConnectionUI : MonoBehaviour
{
    private const string STARTING_GAME_TEXT = "STARTING GAME";
    [SerializeField] private GameObject startScreen;
    [SerializeField] private TMP_Text headingText;

    public void Awake()
    {
        CustomNetworkEvents.AllPlayersConnectedEvent += ChangeText;
    }

    public void OnDestroy()
    {
        CustomNetworkEvents.AllPlayersConnectedEvent -= ChangeText;
    }

    private void ChangeText()
    {
        headingText.text = STARTING_GAME_TEXT;
    }

    public void Disconnected()
    {
        ConnectionManager.TryDisconnect();
        startScreen.SetActive(true);
        gameObject.SetActive(false);
    }
}
