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
    public string fallbackUrl = "https://localhost:7078/";
    public float reconnectDelay = 5f;
    public int maxReconnectAttempts = 5;

    private CanvasGroup canvasGroup;
    private ClientWebSocket webSocket;
    private CancellationTokenSource cancellationTokenSource;
    private bool isConnecting = false;
    private int reconnectAttempts = 0;
    private const float fadeSpeed = 2f;
    private Queue<NotificationItem> notificationQueue = new Queue<NotificationItem>();

    private class NotificationItem
    {
        public string Message { get; set; }
        public float Duration { get; set; }
    }

    private void Awake()
    {
        // Keep this GameObject between scene loads
        DontDestroyOnLoad(gameObject);
        notificationQueue = new Queue<NotificationItem>();
    }

    private void Start()
    {
        Debug.Log("NotificatieController started");

        // Initialize UI components
        InitializeUI();

        // Start WebSocket connection
        cancellationTokenSource = new CancellationTokenSource();
        ConnectToWebSocket();
    }

    private void Update()
    {
        // Process notification queue if not currently showing a notification
        // and if notificationPanel is not null
        if (notificationQueue.Count > 0 && notificationPanel != null && !notificationPanel.activeSelf)
        {
            var item = notificationQueue.Dequeue();
            DisplayNotification(item.Message, item.Duration);
        }
    }


    private void InitializeUI()
    {
        // Setup notification panel and canvas group
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
    }

    private async void ConnectToWebSocket()
    {
        if (isConnecting || (webSocket != null && webSocket.State == WebSocketState.Open))
            return;

        isConnecting = true;

        try
        {
            // Get base URL from WebClient if available
            string baseUrl = fallbackUrl;
            if (webClient != null)
            {
                // Correctly access the WebClient's apiUrl
                baseUrl = webClient.baseUrl;
                Debug.Log($"Using WebClient URL: {baseUrl}");
            }
            else
            {
                Debug.LogWarning($"WebClient not available, using fallback URL: {baseUrl}");
            }

            // Safety check for empty URL
            if (string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = fallbackUrl;
                Debug.LogWarning($"Empty URL from WebClient, using fallback: {baseUrl}");
            }

            // Format WebSocket URL
            string wsUrl = baseUrl.Replace("http://", "ws://").Replace("https://", "wss://");
            if (!wsUrl.EndsWith("/"))
                wsUrl += "/";
            wsUrl += "ws";

            Debug.Log($"Connecting to WebSocket at: {wsUrl}");

            // Get token from SecureUserSession
            string token = SecureUserSession.Instance.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                Debug.LogError("Authentication token not available");
                ScheduleReconnect();
                return;
            }

            // Create new WebSocket
            webSocket = new ClientWebSocket();
            webSocket.Options.SetRequestHeader("Authorization", $"Bearer {token}");

            // Connect to WebSocket with timeout
            using var timeoutCts = new CancellationTokenSource(10000); // 10 second timeout
            await webSocket.ConnectAsync(new Uri(wsUrl), timeoutCts.Token);
            Debug.Log("WebSocket connected successfully");

            // Reset reconnect attempts after successful connection
            reconnectAttempts = 0;

            // Enqueue a success notification
            ShowNotification("Connected to notification service", 2f);

            // Start listening for messages
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
            float delay = reconnectDelay * Mathf.Pow(1.5f, reconnectAttempts - 1); // Exponential backoff
            Debug.Log($"Attempting to reconnect in {delay:F1} seconds... (Attempt {reconnectAttempts}/{maxReconnectAttempts})");
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
            return;

        var buffer = new byte[4096];
        try
        {
            Debug.Log("Starting to listen for WebSocket messages");
            while (webSocket.State == WebSocketState.Open && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                var result = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer),
                    cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.Log($"WebSocket message received: {message}");

                    ProcessWebSocketMessage(message);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    Debug.Log("WebSocket closed by server");
                    break;
                }
            }
        }
        catch (WebSocketException ex)
        {
            Debug.LogError($"WebSocket error while listening: {ex.Message}");
        }
        catch (TaskCanceledException)
        {
            Debug.Log("WebSocket listening canceled");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error listening to WebSocket: {ex.Message}");
        }
        finally
        {
            Debug.Log("WebSocket listener exiting, socket state: " + (webSocket != null ? webSocket.State.ToString() : "null"));
            // Reconnect if disconnected unexpectedly
            if (!cancellationTokenSource.Token.IsCancellationRequested)
            {
                await Task.Delay(1000);
                ConnectToWebSocket();
            }
        }
    }

    private void ProcessWebSocketMessage(string jsonMessage)
    {
        try
        {
            Debug.Log($"Processing message: {jsonMessage}");
            var message = JsonConvert.DeserializeObject<JObject>(jsonMessage);

            if (message == null)
            {
                Debug.LogError("Failed to parse WebSocket message");
                return;
            }

            string messageType = message["type"]?.ToString();
            Debug.Log($"Message type: {messageType}");

            if (messageType == "notification")
            {
                string notificationText = message["message"]?.ToString();
                Debug.Log($"Received notification: {notificationText}");

                if (!string.IsNullOrEmpty(notificationText))
                {
                    ShowNotification(notificationText, 5f);
                }
            }
            else if (messageType == "welcome")
            {
                Debug.Log("Welcome message received from WebSocket server");
                ShowNotification("Connected to notification service", 2f);
            }
            else
            {
                Debug.Log($"Unhandled message type: {messageType}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error processing WebSocket message: {ex.Message}");
        }
    }

    public void ShowNotification(string message, float duration = 3f)
    {
        Debug.Log($"Queueing notification: {message}");

        if (notificationQueue != null)
        {
            notificationQueue.Enqueue(new NotificationItem { Message = message, Duration = duration });
        }
        else
        {
            Debug.LogError("Notification queue is null!");
        }
    }

    private void DisplayNotification(string message, float duration)
    {
        if (notificationPanel == null || notificationText == null)
        {
            Debug.LogError("Notification UI components not set!");
            return;
        }

        Debug.Log($"Displaying notification: {message}");
        StopAllCoroutines();
        notificationText.text = message;
        notificationPanel.SetActive(true);
        StartCoroutine(ShowAndHideNotification(duration));
    }

    private IEnumerator ShowAndHideNotification(float duration)
    {
        // Check if components exist
        if (canvasGroup == null || notificationPanel == null)
        {
            Debug.LogError("Required components missing for notification animation");
            yield break;
        }

        // Fade-in
        canvasGroup.alpha = 0;
        while (canvasGroup.alpha < 1)
        {
            canvasGroup.alpha += Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // Wait
        yield return new WaitForSeconds(duration);

        // Fade-out
        while (canvasGroup != null && canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // Check again before deactivating
        if (notificationPanel != null)
        {
            notificationPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        // Clean up WebSocket resources
        if (cancellationTokenSource != null)
        {
            cancellationTokenSource.Cancel();
            cancellationTokenSource.Dispose();
            cancellationTokenSource = null;
        }

        DisposeWebSocket();
    }

    // Method to manually reconnect (can be called from UI)
    public void ReconnectWebSocket()
    {
        DisposeWebSocket();
        reconnectAttempts = 0;
        ConnectToWebSocket();
    }



    // Method to send a test notification (for debugging)
    public void SendTestNotification(string message = "This is a test notification")
    {
        ShowNotification(message);
    }
}
