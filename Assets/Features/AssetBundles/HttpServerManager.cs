using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HttpServerManager : MonoBehaviour
{
    public int MaxConcurrentRequests = 1;
    public int Port = 50000;
    public bool IsListening;    
    HttpListenerContext currentContext;

    #region Managers
    public CaptureJobsManager CaptureJobsManager;
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        InitHttpListener();
        IsListening = true;        
    }

    void OnDestroy()
    {        
        IsListening = false;
    }

    void InitHttpListener()
    {
        Task.Run(() => {
            HttpListener httpListener = new HttpListener();
            httpListener.AuthenticationSchemes = AuthenticationSchemes.Anonymous;
            httpListener.Prefixes.Add($"http://*:{Port}/");         
            httpListener.Start();
            while(IsListening)
            {
                var context = httpListener.GetContext();
                HandleRequest(context, context.Request);
            }
            httpListener.Stop();
        });
    }

    void HandleRequest(HttpListenerContext context, HttpListenerRequest request)
    {
        currentContext = context;
        currentContext.Response.KeepAlive = true;
        if (request.Url.AbsolutePath == Constants.Routes.RequestRandomShieldGuid)
        {
            try
            {
                //PARSE QUERIES...                
                var guid = CaptureJobsManager.AddCaptureJob(request.Url.Query.Replace("?q=",string.Empty));
                currentContext.Response.StatusCode = (int)HttpStatusCode.OK;
                using (var s = currentContext.Response.OutputStream)
                {                    
                    var buffer = ASCIIEncoding.ASCII.GetBytes(guid.ToString());
                    s.Write(buffer, 0, buffer.Length);
                    
                }
                
            }
            catch (JobAlreadyExistsException)
            {
                currentContext.Response.StatusCode = (int)HttpStatusCode.Accepted;
                using (var s = currentContext.Response.OutputStream)
                {
                    var buffer = ASCIIEncoding.ASCII.GetBytes(nameof(JobAlreadyExistsException));
                    s.Write(buffer, 0, buffer.Length);
                }                
            }
        }
        else if(request.Url.AbsolutePath == Constants.Routes.GetGifByGuid)
        {
            try
            {
                //PARSE QUERIES...
                var captureResult = CaptureJobsManager.GetCaptureResult(new Guid(request.Url.Query.Replace("?q=", string.Empty)));
                if(captureResult.Status == CaptureJobStatus.Completed)
                {
                    currentContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (var s = currentContext.Response.OutputStream)
                    {
                        var buffer = captureResult.Raw;
                        s.Write(buffer, 0, buffer.Length);                        
                    }                    
                }
                else
                {
                    currentContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (var s = currentContext.Response.OutputStream)
                    {
                        var buffer = ASCIIEncoding.ASCII.GetBytes(Enum.GetName(typeof(CaptureJobStatus) ,captureResult.Status));
                        s.Write(buffer, 0, buffer.Length);                        
                    }                    
                }
            }
            catch (FileNotFoundException)
            {
                //TODO
            }
            catch (Exception)
            {
                //TODO
            }
        }
        else
        {
            currentContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            currentContext.Response.Close();
        }
    }
}
