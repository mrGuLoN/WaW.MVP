using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class NetworkUIController : MonoBehaviour
{
    [SerializeField] private GameObject _canvas;
    [SerializeField] private Button _serverButton, _hostButton, _connectButton; 
    void Start()
    {
        _serverButton.onClick?.AddListener(() =>
        {
            NetworkManager.Singleton.StartServer();
            _canvas.SetActive(false);
        });
        _hostButton.onClick?.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            _canvas.SetActive(false);
        });
        _connectButton.onClick?.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            _canvas.SetActive(false);
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
