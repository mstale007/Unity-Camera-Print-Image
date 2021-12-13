using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Media;

public class CameraScript : MonoBehaviour
{
    static WebCamTexture myCam;

    private Texture defaultBackground;

    public RawImage background;
    public AspectRatioFitter fit;

    public Texture2D template;

    public RectTransform rectT; // Assign the UI element which you wanna capture
    // Start is called before the first frame update
    void Start()
    {

        //defaultBackground = background.texture;
        WebCamDevice[] devices = WebCamTexture.devices;

        if (devices.Length==0)
        {
            Debug.Log("Camera Not Detected");
            return;
        }

        //TODO: Change this part before testing
        for (int i = 0; i < devices.Length; i++)
        {
            Debug.Log(devices[i].name);
            if (devices[i].name == "HP Wide Vision HD Camera")
                myCam = new WebCamTexture(devices[i].name, Screen.width,Screen.height);
        }

        if (myCam == null)
        {
            Debug.Log("myCam is null");
            return;
        }

        //GetComponent<Renderer>().material.mainTexture = myCam;

        if (!myCam.isPlaying)
            myCam.Play();

        float ratio = (float)myCam.width / (float)myCam.height;
        fit.aspectRatio = ratio;

        float scaleX = myCam.videoVerticallyMirrored ? 1f : -1f;
        background.rectTransform.localScale = new Vector3(scaleX,1f, 1f);

        RectTransform rt = background.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(myCam.width,rt.sizeDelta.y);
        background.texture=myCam;
        
        //TakeSnapshot();

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.W)) // 0 - left button; 1 - right button; 2 - middle button
        {
            StartCoroutine(takeScreenShot());
            //TakeSnapshot();
        }
        Texture2D cropped_snap;

        int x_snap = myCam.width;
        int y_snap = myCam.height;

        if (x_snap >= 720 && y_snap >= 1080)
        {
            cropped_snap = new Texture2D(720, 1080);
            cropped_snap.SetPixels(myCam.GetPixels(x_snap / 2 - (720 / 2), y_snap / 2 - (1080 / 2), 720, 1080));
        }
        else
        {
            int cropped_height = y_snap;
            int cropped_width = (cropped_height * 2) / 3;
            cropped_snap = new Texture2D(cropped_width, cropped_height);
            cropped_snap.SetPixels(myCam.GetPixels(x_snap / 2 - (cropped_height / 2), 0, cropped_width, cropped_height));
        }

        //background_test.texture = cropped_snap;
        //System.IO.File.WriteAllBytes(Application.persistentDataPath + "/Cropped_.png", cropped_snap.EncodeToPNG());
        /*
        float ratio = (float)myCam.width / (float)myCam.height;
        fit.aspectRatio = ratio;

        float scaleY = myCam.videoVerticallyMirrored ? -1f : 1f;
        background.rectTransform.localScale = new Vector3(1f, scaleY, 1f);

        int orient = -myCam.videoRotationAngle;
        background.rectTransform.localEulerAngles = new Vector3(0, 0, orient);
        */
    }

    //int _CaptureCounter = 0;
    void TakeSnapshot()
    {
        string path = Application.persistentDataPath;
        Debug.Log(path);

        float alph;
        Color col;
        int x_temp = template.width;
        int y_temp = template.height;
        Texture2D result = new Texture2D(template.width, template.height); ;
        Texture2D snap = new Texture2D(myCam.width, myCam.height);
        Texture2D cropped_snap;

        //Copy From template
        result.SetPixels(template.GetPixels());
        result.Apply();

        //Take camera snap
        snap.SetPixels(myCam.GetPixels());
        snap.Apply();

        int x_snap = snap.width;
        int y_snap = snap.height;

        Debug.Log("Template: "+template.width+" "+template.height);
        Debug.Log("Snap: " + x_snap + " " + y_snap);


        
        if (x_snap>=720 && y_snap>=1080)
        {
            cropped_snap = new Texture2D(720, 1080);
            cropped_snap.SetPixels(snap.GetPixels(x_snap/2-(720/2),y_snap/2-(1080/2),720,1080));

            //Overlay template on image
            result =_OverlayImages(cropped_snap, template);
        }
        else
        {
            int cropped_height = snap.height;
            int cropped_width = (cropped_height * 2) / 3;
            cropped_snap = new Texture2D(cropped_width, cropped_height);
            Debug.Log("Cropped: " + cropped_width + " " + cropped_height);
            cropped_snap.SetPixels(snap.GetPixels(x_snap / 2 - (cropped_height / 2), 0, cropped_width, cropped_height));
            System.IO.File.WriteAllBytes(path + "/Cropped_.png", cropped_snap.EncodeToPNG());
            //Overlay snap on template
            int x = x_temp / 2 + x_snap / 2;
            while (x > x_temp / 2 - x_snap / 2)
            {
                x--;
                int y = y_temp / 2 + y_snap / 2;

                //y = snap.height;
                while (y > y_temp / 2 - y_snap / 2)
                {
                    y--;
                    //use the alpha channel of 2nd image to mix the colors
                    alph = template.GetPixel(x,y).a;
                    // remove this next line if you don't want alpha cutoff!
                    //if (alph > .9f) { alph = 1f; } else { alph = 0; }
                    col = template.GetPixel(x,y);
                    col = col * alph;
                    alph = 1f - alph;
                    col = col + snap.GetPixel(x - x_temp / 2 + x_snap / 2, y - y_temp / 2 + y_snap / 2) * alph;
                    result.SetPixel(x, y, col);
                }
            }
            result.Apply();
        }
        
        System.IO.File.WriteAllBytes(path + "/Selfie.png", result.EncodeToPNG());

        /**
        //Backup Image
        string extra = DateTime.Now.ToString();
        extra=extra.Replace(" ", "");
        extra=extra.Replace("-", "");
        extra=extra.Replace(":", "");
        System.IO.File.WriteAllBytes(path + "/Selfie_"+extra + ".png", result.EncodeToPNG());
        //++_CaptureCounter;
        **/
        Debug.Log("Saved!");
        //printImageCmd(path + "/Selfie.png");
        //printImage(path + "/Selfie.png");
    }

    void printImageCmd(string img_path)
    {
        string printerName = "Microsoft Print to PDF";
        string _filePath = img_path;
        //"C:\windows\system32\mspaint.exe" /p "C:\Users\Jason\Documents\UnityProjects\Test-Printing\Assets\test.png"
        string _cmd = "C:\\windows\\system32\\mspaint.exe /p "+_filePath.Replace(@"/",@"\");
        try
        {
            Debug.Log(_cmd);
            System.Diagnostics.Process myProcess = new System.Diagnostics.Process();
            //myProcess.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            myProcess.StartInfo.CreateNoWindow = true;
            myProcess.StartInfo.UseShellExecute = false;
            myProcess.StartInfo.FileName = "cmd.exe";
            myProcess.StartInfo.Arguments = "/c " + _cmd;
            myProcess.EnableRaisingEvents = true;
            myProcess.Start();
            myProcess.WaitForExit();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.Log(e);
        }
    }

    public IEnumerator takeScreenShot()
    {
        yield return new WaitForEndOfFrame(); // it must be a coroutine 

        Vector2 temp = rectT.transform.position;

        int width = System.Convert.ToInt32(rectT.rect.width);
        int height = System.Convert.ToInt32(rectT.rect.height);

        /*
        int height =(int) whiteBackground.rect.height;
        int width =(int) whiteBackground.rect.width;
        */
        var startX = temp.x - width / 2;
        var startY = temp.y - height / 2;

        var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(startX, startY, width, height), 0, 0);
        tex.Apply();


        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/Ss.png", tex.EncodeToPNG());
        Debug.Log("Saved SS");

        Destroy(tex);

    }
    void printImage(string path)
    {
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        process.StartInfo.CreateNoWindow = false;
        process.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
        process.StartInfo.UseShellExecute = true;
        process.StartInfo.FileName = path;
        process.StartInfo.Verb = "print";

        process.Start();
    }

    Texture2D _OverlayImages(Texture2D background,Texture2D foreground)
    {
        float alph;
        Color col;
        Texture2D result;

        //Copy From template
        int x_back = background.width;
        int y_back = background.height;
        result = new Texture2D(x_back, y_back);

        int x = x_back;
        while (x > 0)
        {
            x--;
            int y = y_back;

            //y = snap.height;
            while (y > 0)
            {
                y--;
                //use the alpha channel of 2nd image to mix the colors
                alph = foreground.GetPixel(x, y).a;
                // remove this next line if you don't want alpha cutoff!
                //if (alph > .9f) { alph = 1f; } else { alph = 0; }
                col = foreground.GetPixel(x, y);
                col = col * alph;
                alph = 1f - alph;
                col = col + background.GetPixel(x, y) * alph;
                result.SetPixel(x, y, col);
            }
        }
        result.Apply();

        return result;
    }

}
