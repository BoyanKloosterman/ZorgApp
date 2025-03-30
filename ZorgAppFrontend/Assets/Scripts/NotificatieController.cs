using UnityEngine;
using TMPro;
using System.Collections;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Net.WebSockets;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;

public class NotificatieController : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI notificationText;
    public GameObject notificationPanel;

    [Header("WebSocket Settings")]
    public WebClient webClient;
    public string fallbackUrl = "https://localhost:7078";
    public float reconnectDelay = 5f;
    public int maxReconnectAttempts = 5;

    private CanvasGroup canvasGroup;
    private ClientWebSocket webSocket;
    private CancellationTokenSource cancellationTokenSource;
    private bool isConnecting = false;
    private int reconnectAttempts = 0;
    private const float fadeSpeed = 2f;
    private Queue<NotificationItem> notificationQueue = new Queue<NotificationItem>();
    private bool isShowingNotification = false;

    private class NotificationItem
    {
        public string Message { get; set; }
        public float Duration { get; set; }
    }

    private void Awake()
    {
        notificationQueue = new Queue<NotificationItem>();
    }

    private void Start()
    {
        InitializeUI();
        cancellationTokenSource = new CancellationTokenSource();
        ConnectToWebSocket();
    }

    private void Update()
    {
        if (notificationQueue.Count > 0 && !isShowingNotification)
        {
            try
            {
                var item = notificationQueue.Dequeue();
                DisplayNotification(item.Message, item.Duration);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error processing notification from queue: {ex.Message}");
            }
        }
    }

    private void InitializeUI()
    {
        if (notificationPanel != null)
        {
            canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }
            notificationPanel.SetActive(false);
            canvasGroup.alpha = 0;
        }
        else
        {
            Debug.LogError("Notification panel not assigned! Please assign it in the Inspector.");
        }

        if (notificationText == null)
        {
            Debug.LogError("Notification text not assigned! Please assign a TextMeshProUGUI component in the Inspector.");
        }
    }

    private async void ConnectToWebSocket()
    {
        if (isConnecting || (webSocket != null && webSocket.State == WebSocketState.Open))
            return;

        isConnecting = true;

        try
        {
            string baseUrl = fallbackUrl;
            if (webClient != null)
            {
                baseUrl = webClient.baseUrl;
            }

            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = fallbackUrl;
            }

            string wsUrl = baseUrl.Replace("http://", "ws://").Replace("https://", "wss://");
            if (!wsUrl.EndsWith("/"))
                wsUrl += "/";
            wsUrl += "ws";

            string token = SecureUserSession.Instance.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("Authentication token not available");
                ScheduleReconnect();
                return;
            }

            webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");

            using var timeoutCts = new CancellationTokenSource(10000);
            await webSocket.ConnectAsync(new Uri(wsUrl), timeoutCts.Token);

            reconnectAttempts = 0;
            ListenForMessages();
        }
        catch (TaskCanceledException)
        {
            Debug.LogError("WebSocket connection timed out");
            ScheduleReconnect();
        }
        catch (Exception ex)
        {
            Debug.LogError($"WebSocket connection error: {ex.Message}");
            if (ex.InnerException != null)
            {
                Debug.LogError($"Inner exception: {ex.InnerException.Message}");
            }

            DisposeWebSocket();
            ScheduleReconnect();
        }
        finally
        {
            isConnecting = false;
        }
    }

    private void DisposeWebSocket()
    {
        if (webSocket != null)
        {
            try
            {
                webSocket.Dispose();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error disposing WebSocket: {ex.Message}");
            }
            webSocket = null;
        }
    }

    private async void ScheduleReconnect()
    {
        reconnectAttempts++;
        if (reconnectAttempts < maxReconnectAttempts)
        {
            float delay = reconnectDelay * Mathf.Pow(1.5f, reconnectAttempts - 1);
            await Task.Delay((int)(delay * 1000));
            ConnectToWebSocket();
        }
        else
        {
            Debug.LogError("Maximum reconnection attempts reached. Giving up.");
        }
    }

    private async void ListenForMessages()
    {
        if (webSocket == null || webSocket.State != WebSocketState.Open)
        {
            return;
        }

        if (cancellationTokenSource == null)
        {
            cancellationTokenSource = new CancellationTokenSource();
        }

        var buffer = new byte[4096];
        try
        {
            while (webSocket != null && webSocket.State == WebSocketState.Open &&
                   cancellationTokenSource != null && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                CancellationTokenSource receiveTimeoutCts = null;
                CancellationTokenSource linkedCts = null;

                try
                {
                    receiveTimeoutCts = new CancellationTokenSource();

                    if (cancellationTokenSource != null)
                    {
                        linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
                            cancellationTokenSource.Token, receiveTimeoutCts.Token);
                    }
                    else
                    {
                        linkedCts = CancellationTokenSource.CreateLinkedTokenSource(receiveTimeoutCts.Token);
                    }

                    receiveTimeoutCts.CancelAfter(30000);

                    if (webSocket == null || webSocket.State != WebSocketState.Open)
                    {
                        break;
                    }

                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        linkedCts.Token);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        ProcessWebSocketMessage(message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        break;
                    }
                }
                catch (OperationCanceledException)
                {
                    if (receiveTimeoutCts != null && receiveTimeoutCts.IsCancellationRequested &&
                        (cancellationTokenSource == null || !cancellationTokenSource.IsCancellationRequested))
                    {
                        try
                        {
                            if (webSocket != null && webSocket.State == WebSocketState.Open)
                            {
                                continue;
                            }
                            else
                            {
                                break;
                            }
                        }
                        catch (Exception ex)
                        {
                            break;
                        }
                    }
                    else if (cancellationTokenSource != null && cancellationTokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                }
                finally
                {
                    try
                    {
                        linkedCts?.Dispose();
                        receiveTimeoutCts?.Dispose();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error disposing token sources: {ex.Message}");
                    }
                }
            }
        }
        catch (WebSocketException ex)
        {
            Debug.LogError($"WebSocket error while listening: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Debug.Log("WebSocket listening task canceled");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error listening to WebSocket: {ex.Message}");
            Debug.LogException(ex);
        }
        finally
        {
            if (cancellationTokenSource != null && !cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    DisposeWebSocket();
                    await Task.Delay(1000);
                    ConnectToWebSocket();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error during reconnection sequence: {ex.Message}");
                }
            }
        }
    }

    private void ProcessWebSocketMessage(string jsonMessage)
    {
        try
        {
            var message = JsonConvert.DeserializeObject<JObject>(jsonMessage);

            if (message == null)
            {
                Debug.LogError("Failed to parse WebSocket message");
                return;
            }

            string messageType = message["type"]?.ToString();

            if (messageType == "notification")
            {
                string notificationText = message["message"]?.ToString();

                if (!string.IsNullOrEmpty(notificationText))
                {
                    ShowNotification(notificationText, 5f);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing WebSocket message: {ex.Message}");
        }
    }

    public void ShowNotification(string message, float duration = 3f)
    {
        if (message == null)
        {
            Debug.LogError("Tried to show a null notification message");
            return;
        }

        notificationQueue.Enqueue(new NotificationItem { Message = message, Duration = duration });
    }

    private void DisplayNotification(string message, float duration)
    {
        if (notificationPanel == null)
        {
            Debug.LogError("Notification panel is null!");
            return;
        }

        if (notificationText == null)
        {
            Debug.LogError("Notification text is null!");
            return;
        }

        isShowingNotification = true;
        notificationPanel.SetActive(true);

        if (canvasGroup == null)
        {
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0;
        notificationText.text = message;
        StopAllCoroutines();
        StartCoroutine(ShowAndHideNotification(duration));
    }

    private IEnumerator ShowAndHideNotification(float duration)
    {
        if (notificationPanel == null || canvasGroup == null)
        {
            Debug.LogError("Missing notification components!");
            isShowingNotification = false;
            yield break;
        }

        canvasGroup.alpha = 0;
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        yield return new WaitForSeconds(duration);

        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        notificationPanel.SetActive(false);
        isShowingNotification = false;
    }

    private void OnDestroy()
    {
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        DisposeWebSocket();
    }

    public void ReconnectWebSocket()
    {
        DisposeWebSocket();
        reconnectAttempts = 0;
        ConnectToWebSocket();
    }
}
