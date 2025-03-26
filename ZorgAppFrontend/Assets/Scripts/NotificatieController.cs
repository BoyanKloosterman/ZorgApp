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
    public string fallbackUrl = "https://localhost:7078"; // Fallback URL if webClient isn't set
    public float reconnectDelay = 5f;
    public int maxReconnectAttempts = 5;

    [Header("Debug")]
    public bool debugMode = true;
    public bool forceShowNotification = false;

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
        // Keep this GameObject between scene loads
        DontDestroyOnLoad(gameObject);

        // Ensure the queue is initialized
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

        // Test notification for debugging
        if (debugMode)
        {
            Debug.Log("Debug mode active, sending test notification");
            Invoke("ShowTestNotification", 2f);
        }
    }

    private void Update()
    {
        // Process notification queue if not currently showing a notification
        if (notificationQueue.Count > 0 && !isShowingNotification)
        {
            Debug.Log($"Processing notification from queue. Queue size: {notificationQueue.Count}");

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

        // Force show notification for debugging
        if (forceShowNotification)
        {
            forceShowNotification = false;
            ShowTestNotification();
        }
    }

    private void InitializeUI()
    {
        // Setup notification panel and canvas group
        if (notificationPanel != null)
        {
            Debug.Log("Initializing notification panel");
            canvasGroup = notificationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.Log("Adding CanvasGroup component to notification panel");
                canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
            }
            notificationPanel.SetActive(false);
            canvasGroup.alpha = 0;
        }
        else
        {
            Debug.LogError("Notification panel not assigned! Please assign it in the Inspector.");
        }

        // Check notification text
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

        Debug.Log($"Displaying notification: {message}");

        // Set the flag to prevent multiple notifications
        isShowingNotification = true;

        // Ensure the panel is active but the canvasGroup is transparent
        notificationPanel.SetActive(true);

        if (canvasGroup == null)
        {
            Debug.LogWarning("CanvasGroup is null, creating one");
            canvasGroup = notificationPanel.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = 0;

        // Set the text
        notificationText.text = message;

        // Start the coroutine
        StopAllCoroutines();
        StartCoroutine(ShowAndHideNotification(duration));
    }

    private IEnumerator ShowAndHideNotification(float duration)
    {
        Debug.Log("Starting notification animation");

        // Ensure required components exist
        if (notificationPanel == null || canvasGroup == null)
        {
            Debug.LogError("Missing notification components!");
            isShowingNotification = false;
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
        Debug.Log($"Notification visible, waiting for {duration} seconds");
        yield return new WaitForSeconds(duration);

        // Fade-out
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * fadeSpeed;
            yield return null;
        }

        // Hide the panel
        notificationPanel.SetActive(false);

        // Reset the flag
        isShowingNotification = false;

        Debug.Log("Notification animation completed");
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
    public void ShowTestNotification(string message = "This is a test notification")
    {
        Debug.Log("Showing test notification");
        ShowNotification(message);
    }
}
