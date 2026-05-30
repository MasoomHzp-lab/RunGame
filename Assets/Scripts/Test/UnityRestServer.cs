using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;

public class UnityRestServer : MonoBehaviour
{
    [SerializeField] private TherapyCommandAPI api;
    [SerializeField] private string urlPrefix = "http://localhost:8080/";
    [SerializeField] private bool autoStart = true;
    [SerializeField] private int mainThreadTimeoutMs = 3000;

    private HttpListener listener;
    private Thread serverThread;
    private volatile bool isRunning;

    private void Reset()
    {
        if (api == null)
            api = FindFirstObjectByType<TherapyCommandAPI>();
    }

    private void Awake()
    {
        if (api == null)
            api = FindFirstObjectByType<TherapyCommandAPI>();
    }

    private void Start()
    {
        if (autoStart)
            StartServer();
    }

    private void OnDestroy()
    {
        StopServer();
    }

    public void StartServer()
    {
        if (isRunning)
            return;

        try
        {
            listener = new HttpListener();
            listener.Prefixes.Add(urlPrefix);
            listener.Start();

            isRunning = true;
            serverThread = new Thread(ListenLoop);
            serverThread.IsBackground = true;
            serverThread.Start();

            Debug.Log("REST server started at " + urlPrefix);
        }
        catch (Exception ex)
        {
            Debug.LogError("REST server start failed: " + ex);
        }
    }

    public void StopServer()
    {
        isRunning = false;

        try
        {
            if (listener != null)
            {
                if (listener.IsListening)
                    listener.Stop();

                listener.Close();
                listener = null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("REST server stop warning: " + ex.Message);
        }

        try
        {
            if (serverThread != null && serverThread.IsAlive)
                serverThread.Join(300);
        }
        catch (Exception ex)
        {
            Debug.LogWarning("REST thread join warning: " + ex.Message);
        }

        serverThread = null;
    }

    private void ListenLoop()
    {
        while (isRunning && listener != null)
        {
            try
            {
                HttpListenerContext context = listener.GetContext();
                HandleRequest(context);
            }
            catch (HttpListenerException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (Exception ex)
            {
                Debug.LogWarning("REST loop error: " + ex.Message);
            }
        }
    }

    private void HandleRequest(HttpListenerContext context)
    {
        if (context.Request.HttpMethod == "OPTIONS")
        {
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, OPTIONS");
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type");
            context.Response.StatusCode = 200;
            context.Response.Close();
            return;
        }

        string path = context.Request.Url.AbsolutePath.TrimEnd('/').ToLowerInvariant();
        if (string.IsNullOrEmpty(path))
            path = "/";

        try
        {
            switch (path)
            {
                case "/":
                    WriteJson(context, "{\"ok\":true,\"service\":\"unity-therapy-api\",\"status\":\"online\"}");
                    return;

                case "/start":
                    DispatchToMainThread(context, () =>
                    {
                        api?.StartSession();
                        return ApiOk("start", "session-started");
                    });
                    return;

                case "/pause":
                    DispatchToMainThread(context, () =>
                    {
                        api?.PauseSession();
                        return ApiOk("pause", "session-paused");
                    });
                    return;

                case "/resume":
                    DispatchToMainThread(context, () =>
                    {
                        api?.ResumeSession();
                        return ApiOk("resume", "session-resumed");
                    });
                    return;

                case "/stop":
                    DispatchToMainThread(context, () =>
                    {
                        api?.StopRun();
                        return ApiOk("stop", "run-queue-cleared-and-player-stopped");
                    });
                    return;

                case "/session/stop":
                    DispatchToMainThread(context, () =>
                    {
                        api?.StopSession();
                        return ApiOk("session-stop", "session-finished");
                    });
                    return;

                case "/reset":
                    DispatchToMainThread(context, () =>
                    {
                        api?.ResetSession();
                        return ApiOk("reset", "session-reset");
                    });
                    return;

                case "/emergency-stop":
                    DispatchToMainThread(context, () =>
                    {
                        api?.EmergencyStop();
                        return ApiOk("emergency-stop", "emergency-stop-triggered");
                    });
                    return;

                case "/sleepmode":
                    DispatchToMainThread(context, () =>
                    {
                        api?.SleepMode();
                        return ApiOk("sleepmode", "game-is-now-sleeping");
                    });
                    return;

                case "/startfreshgame":
                    DispatchToMainThread(context, () =>
                    {
                        api?.StartFreshGame();
                        return ApiOk("startfreshgame", "game-restarted");
                    });
                    return;

                case "/run":
                    HandleRunRequest(context);
                    return;

                case "/state":
                    DispatchToMainThread(context, () =>
                    {
                        TherapyStateSnapshot snap = api != null ? api.GetStateSnapshot() : new TherapyStateSnapshot
                        {
                            ok = false,
                            state = "Unavailable",
                            score = 0,
                            progress = 0f,
                            queuedCommands = 0,
                            isProcessing = false,
                            nextExpectedFoot = "Left"
                        };

                        return JsonUtility.ToJson(snap);
                    });
                    return;

                default:
                    WriteJson(context, ApiError("unknown-endpoint"), 404);
                    return;
            }
        }
        catch (Exception ex)
        {
            WriteJson(context, ApiError("server-error", ex.Message), 500);
        }
    }

    private void HandleRunRequest(HttpListenerContext context)
    {
        if (context.Request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
        {
            string body = ReadBody(context.Request);
            RunHttpRequest request = null;

            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    request = JsonUtility.FromJson<RunHttpRequest>(body);

                    // جلوگیری از تداخل نام متغیر با بخش GET
                    if (TryReadFootSideFromJson(body, out short parsedSideFromBody))
                    {
                        request.hasFootSide = true;
                        request.footSide = parsedSideFromBody;
                    }
                }
                catch (Exception ex)
                {
                    WriteJson(context, ApiError("invalid-json", ex.Message), 400);
                    return;
                }
            }

            if (request == null)
            {
                WriteJson(context, ApiError("invalid-body", "request body is empty or invalid"), 400);
                return;
            }

            DispatchToMainThread(context, () =>
            {
                return ExecuteRunRequest(request);
            });

            return;
        }

        int time = GetInt(context.Request, "time", 500);
        short power = (short)GetInt(context.Request, "power", 50);
        short threshold = (short)GetInt(context.Request, "threshold", 40);
        string sideRaw = context.Request.QueryString["side"];

        bool hasSide = TryParseFootSide(sideRaw, out short parsedSide);

        RunHttpRequest requestFromQuery = new RunHttpRequest
        {
            timeMs = time,
            footPower = power,
            threshold = threshold,
            hasFootSide = hasSide,
            footSide = parsedSide
        };

        DispatchToMainThread(context, () =>
        {
            return ExecuteRunRequest(requestFromQuery);
        });
    }

    private string ExecuteRunRequest(RunHttpRequest request)
    {
        if (api == null)
            return ApiError("api-not-set");

        bool ok;
        string message;

        if (request.hasFootSide)
            ok = api.Run(request.timeMs, request.footPower, request.threshold, request.footSide, out message);
        else
            ok = api.Run(request.timeMs, request.footPower, request.threshold, out message);

        if (!ok)
            return ApiError("run-rejected", message);

        TherapyStateSnapshot snap = api.GetStateSnapshot();
        RunAcceptedResponse response = new RunAcceptedResponse
        {
            ok = true,
            action = "run",
            message = message,
            queuedCommands = snap.queuedCommands,
            isProcessing = snap.isProcessing,
            nextExpectedFoot = snap.nextExpectedFoot
        };

        return JsonUtility.ToJson(response);
    }

    private bool TryReadFootSideFromJson(string json, out short footSide)
    {
        footSide = 0;

        if (string.IsNullOrWhiteSpace(json))
            return false;

        Match match = Regex.Match(
            json,
            "\"(?:footSide|side)\"\\s*:\\s*(\"[^\"]+\"|-?\\d+)",
            RegexOptions.IgnoreCase
        );

        if (!match.Success)
            return false;

        string raw = match.Groups[1].Value.Trim().Trim('"');
        return TryParseFootSide(raw, out footSide);
    }

    private bool TryParseFootSide(string raw, out short footSide)
    {
        footSide = 0;

        if (string.IsNullOrWhiteSpace(raw))
            return false;

        raw = raw.Trim().ToLowerInvariant();

        if (raw == "left" || raw == "l" || raw == "0" || raw == "2")
        {
            footSide = 2;
            return true;
        }

        if (raw == "right" || raw == "r" || raw == "1")
        {
            footSide = 1;
            return true;
        }

        return false;
    }

    private void DispatchToMainThread(HttpListenerContext context, Func<string> action)
    {
        if (!MainThreadDispatcher.HasInstance)
        {
            WriteJson(context, ApiError("main-thread-dispatcher-missing"), 500);
            return;
        }

        string json = null;
        int statusCode = 200;
        Exception actionException = null;

        using (ManualResetEventSlim done = new ManualResetEventSlim(false))
        {
            MainThreadDispatcher.EnqueueStatic(() =>
            {
                try
                {
                    json = action.Invoke();
                }
                catch (Exception ex)
                {
                    actionException = ex;
                    statusCode = 500;
                    json = ApiError("main-thread-action-failed", ex.Message);
                }
                finally
                {
                    done.Set();
                }
            });

            if (!done.Wait(mainThreadTimeoutMs))
            {
                WriteJson(context, ApiError("main-thread-timeout"), 504);
                return;
            }
        }

        WriteJson(context, json ?? ApiError("empty-response"), statusCode);
    }

    private int GetInt(HttpListenerRequest request, string key, int defaultValue)
    {
        string raw = request.QueryString[key];
        return int.TryParse(raw, out int value) ? value : defaultValue;
    }

    private string ReadBody(HttpListenerRequest request)
    {
        using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
        {
            return reader.ReadToEnd();
        }
    }

    private void WriteJson(HttpListenerContext context, string json, int statusCode = 200)
    {
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        context.Response.ContentEncoding = Encoding.UTF8;
        context.Response.ContentLength64 = buffer.Length;
        context.Response.AddHeader("Access-Control-Allow-Origin", "*");

        try
        {
            context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        }
        finally
        {
            context.Response.OutputStream.Close();
        }
    }

    private string ApiOk(string action, string message)
    {
        BasicApiResponse response = new BasicApiResponse
        {
            ok = true,
            action = action,
            message = message
        };

        return JsonUtility.ToJson(response);
    }

    private string ApiError(string errorCode, string detail = null)
    {
        ErrorApiResponse response = new ErrorApiResponse
        {
            ok = false,
            error = errorCode,
            detail = detail ?? string.Empty
        };

        return JsonUtility.ToJson(response);
    }
}

[Serializable]
public class RunHttpRequest
{
    public int timeMs;
    public short footPower;
    public short threshold;
    public bool hasFootSide;
    public short footSide;
}

[Serializable]
public class BasicApiResponse
{
    public bool ok;
    public string action;
    public string message;
}

[Serializable]
public class ErrorApiResponse
{
    public bool ok;
    public string error;
    public string detail;
}

[Serializable]
public class RunAcceptedResponse
{
    public bool ok;
    public string action;
    public string message;
    public int queuedCommands;
    public bool isProcessing;
    public string nextExpectedFoot;
}