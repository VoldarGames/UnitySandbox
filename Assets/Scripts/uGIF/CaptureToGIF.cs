using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

using System.Threading;
using System;

namespace uGIF
{
    public class CaptureToGIF : MonoBehaviour
    {
        string path; 
        public float frameRate = 15;
        public bool capture;
        public int downscale = 1;
        public float captureTime = 10;
        public bool useBilinearScaling = true;
        public bool DeletePngsAfterGifCreation = false;

        public RenderTexture MyRenderTexture;

        [System.NonSerialized]
        public byte[] bytes = null;

        void Start()
        {
            period = 1f / frameRate;
            //colorBuffer = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            colorBuffer = new Texture2D(256, 256, TextureFormat.RGB24, false);

            //
            RenderTexture.active = MyRenderTexture;
            colorBuffer.ReadPixels(new Rect(0, 0, colorBuffer.width, colorBuffer.height), 0, 0);
            colorBuffer.Apply();
            //

            startTime = Time.time;
            Debug.Log("Start rendering camera GIF");            
        }

        void Update()
        {
            Camera.main.Render();
        }

        public void Encode()
        {
            Utils.CreateDirectory("pngs");
            for (int i = 0; i < pngs.Count; i++)
            {
                System.IO.File.WriteAllBytes($"pngs/{i}.png", pngs[i]);
            }

            System.Diagnostics.Process ffmpeg = new System.Diagnostics.Process();
            ffmpeg.StartInfo.FileName = "C:\\Windows\\system32\\cmd.exe";
            ffmpeg.StartInfo.Arguments = "/c " + "cd " + Utils.ProjectPath + " && ffmpeg\\ffmpeg.exe -i pngs\\%d.png -f gif -y out.gif";
            ffmpeg.Start();
            
            path = "Test.gif";
            bytes = null;
            var thread = new Thread(_Encode);
            thread.Start();
            StartCoroutine(WaitForBytes());

            if(DeletePngsAfterGifCreation) StartCoroutine(DeletePngsDirectory());
        }

        IEnumerator DeletePngsDirectory()
        {
            yield return new WaitForSeconds(10);
            Directory.Delete("pngs", true);
        }

        IEnumerator WaitForBytes()
        {
            while (bytes == null) yield return null;
            System.IO.File.WriteAllBytes(path, bytes);
            bytes = null;
        }

        public void _Encode()
        {
            capture = false;

            var ge = new GIFEncoder();
            ge.useGlobalColorTable = true;
            ge.repeat = 0;
            ge.FPS = frameRate;
            ge.transparent = new Color32(255, 0, 255, 255);
            ge.dispose = 1;

            var stream = new MemoryStream();
            ge.Start(stream);
            foreach (var f in frames)
            {
                if (downscale != 1)
                {
                    if (useBilinearScaling)
                    {
                        f.ResizeBilinear(f.width / downscale, f.height / downscale);
                    }
                    else
                    {
                        f.Resize(downscale);
                    }
                }
                f.Flip();
                ge.AddFrame(f);
            }
            ge.Finish();
            bytes = stream.GetBuffer();
            stream.Close();
            Debug.Log("Render Finished!");
        }

        void OnPostRender()
        {
            if (capture)
            {
                T += Time.deltaTime;
                if (T >= period)
                {
                    T = 0;
                    colorBuffer.ReadPixels(new Rect(0, 0, colorBuffer.width, colorBuffer.height), 0, 0, false);
                    frames.Add(new Image(colorBuffer));

                    //
                    pngs.Add(ImageConversion.EncodeToPNG(colorBuffer));
                    //
                }
                if (Time.time > (startTime + captureTime))
                {
                    capture = false;
                    Encode();
                }
            }
        }

        List<Image> frames = new List<Image>();
        Texture2D colorBuffer;
        float period;
        float T = 0;
        float startTime = 0;

        List<byte[]> pngs = new List<byte[]>();
    }
}