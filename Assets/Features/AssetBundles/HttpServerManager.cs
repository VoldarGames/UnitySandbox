﻿using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class HttpServerManager : MonoBehaviour
{
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
                        var buffer = captureResult.RawGif;
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
                currentContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            catch (Exception)
            {
                currentContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
        else if (request.Url.AbsolutePath == Constants.Routes.GetPngByGuid)
        {
            try
            {
                //PARSE QUERIES...
                var captureResult = CaptureJobsManager.GetCaptureResult(new Guid(request.Url.Query.Replace("?q=", string.Empty)));
                if (captureResult.Status == CaptureJobStatus.Completed)
                {
                    currentContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (var s = currentContext.Response.OutputStream)
                    {
                        var buffer = captureResult.RawPng;
                        s.Write(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    currentContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (var s = currentContext.Response.OutputStream)
                    {
                        var buffer = ASCIIEncoding.ASCII.GetBytes(Enum.GetName(typeof(CaptureJobStatus), captureResult.Status));
                        s.Write(buffer, 0, buffer.Length);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                currentContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
            catch (Exception)
            {
                currentContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
        else if (request.Url.AbsolutePath == Constants.Routes.GetShieldByUserId)
        {
            try
            {
                //PARSE QUERIES...
                var fileShieldPath = $"{request.Url.Query.Replace("?q=", string.Empty)}.shield";
                if(File.Exists(fileShieldPath))
                {
                    var shieldJson = File.ReadAllText(fileShieldPath);

                    currentContext.Response.StatusCode = (int)HttpStatusCode.OK;
                    using (var s = currentContext.Response.OutputStream)
                    {
                        var buffer = ASCIIEncoding.ASCII.GetBytes(shieldJson);
                        s.Write(buffer, 0, buffer.Length);
                    }
                }
                else
                {
                    Logger.Log($"Current user id {fileShieldPath} doesnt have any shield generated", Logger.LogLevel.Warning);
                    currentContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }

            }
            catch(Exception e)
            {
                Logger.Log(e.Message, Logger.LogLevel.Error);
                currentContext.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            }
        }
        else
        {
            Logger.Log($"Route {request.Url.AbsolutePath} Not found", Logger.LogLevel.Error);
            currentContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
            currentContext.Response.Close();
        }
    }
}
