using UnityEngine;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Collections;
using static ServerListPacket;

public class LoginClient : DefaultClient {
    // Crypt
    public static byte[] STATIC_BLOWFISH_KEY = {
        (byte) 0x6b,
        (byte) 0x60,
        (byte) 0xcb,
        (byte) 0x5b,
        (byte) 0x82,
        (byte) 0xce,
        (byte) 0x90,
        (byte) 0xb1,
        (byte) 0xcc,
        (byte) 0x2b,
        (byte) 0x6c,
        (byte) 0x55,
        (byte) 0x6c,
        (byte) 0x6c,
        (byte) 0x6c,
        (byte) 0x6c
    };


    [SerializeField] protected string _account;
    [SerializeField] protected string _password;

    public string Account { get { return _account; } set { _account = value; } }
    public string Password { get { return _password; } set { _password = value; } }

    private LoginClientPacketHandler clientPacketHandler;
    private LoginServerPacketHandler serverPacketHandler;

    public LoginClientPacketHandler ClientPacketHandler { get { return clientPacketHandler; } }
    public LoginServerPacketHandler ServerPacketHandler { get { return serverPacketHandler; } }


    private static LoginClient _instance;
    public static LoginClient Instance { get { return _instance; } }

    private void Awake() {
        if (_instance == null) {
            _instance = this;
        } else if (_instance != this) {
            Destroy(this);
        }
    }

    protected override void CreateAsyncClient() {
        clientPacketHandler = new LoginClientPacketHandler();
        serverPacketHandler = new LoginServerPacketHandler();

        _client = new AsynchronousClient(_serverIp, _serverPort, this, clientPacketHandler, serverPacketHandler, true);
    }

    protected override void WhileConnecting() {
        base.WhileConnecting();
        SetBlowFishKey(STATIC_BLOWFISH_KEY);
    }

    protected override void OnConnectionSuccess() {
        base.OnConnectionSuccess();

        Debug.Log("Connected to LoginServer");

        GameManager.Instance.OnLoginServerConnected();
    }

    public override void OnConnectionFailed() {
        base.OnConnectionFailed();
    }

    public override void OnAuthAllowed() {
        Debug.Log("Authed to LoginServer");
        GameManager.Instance.OnLoginServerAuthAllowed();
    }

    public void OnPlayOk() {
        GameManager.Instance.OnLoginServerPlayOk();

        Disconnect();
    }

    public void OnServerListReceived(byte lastServer, List<ServerData> serverData, Dictionary<int, int> charsOnServers) {
        GameManager.Instance.OnReceivedServerList(lastServer, serverData, charsOnServers);
    }

    public void OnServerSelected(int serverId) {
        clientPacketHandler.SendRequestServerLogin(serverId);
    }

    public override void OnDisconnect() {
        base.OnDisconnect();

        if (GameManager.Instance.GameState == GameState.READY_TO_CONNECT) {
            GameClient.Instance.Connect();
        }

        Debug.Log("Disconnected from LoginServer.");
    }
}
