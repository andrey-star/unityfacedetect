using UnityEngine;
using UnityEngine.UI;
using System.IO;

public partial class VideoCapture : MonoBehaviour
{
    public class DoubleTexture
    {
        Texture2D texture;
        Texture2D edge;
        public Texture2D Texture
        {
            get
            {
                return this.texture;
            }
        }

        public Texture2D Edge
        {
            get
            {
                return this.edge;
            }
        }

        public DoubleTexture(Texture2D texture, Texture2D edge)
        {
            this.texture = texture;
            this.edge = edge;
        }


    }

    public RawImage rawimage;
    public WebCamTexture _CamTex;
    public Texture2D snap;
    public Texture2D snap_prev;
    public Texture2D snap_cor;
    public Texture2D snap_bw;
    public int FPS = 25;
    public int facesize = 0;
    public int updateFrequency = 6;
    public int deltaUpdate = 6;
    public int firstImageAverage;
    public int lastImageAverage;
    public int lastZero = 0;
    public string rotationState = "";
    private string _SavePath = "D:/WebCam/";
    private int _CaptureCounter = 0;
    float angle = 0;
    int rotateSpeed = 7;
    public int prevMoment = 0;
    public int curMoment = 0;
    public string[] strArr = new string[240];
    public DoubleTexture snap1;
    public DoubleTexture snap2;
    public Texture2D diff;
    public Texture2D edgeSnap;
    public Texture2D edgeXSnap;
    public Texture2D edgeYSnap;
    public Texture2D edgeColorSnap;
    public Texture2D edgeSobel;
    public Texture2D blurSnap;
    public int[,] edgeXMatrix = new int[,] { { -1, 0, 1 }, { -2, 0, 2 }, { -1, 0, 1 } };
    public int[,] edgeYMatrix = new int[,] { { 1, 2, 1 }, { 0, 0, 0 }, { -1, -2, -1 } };
    public int[,] blurMatrix = new int[,] { { 2, 4, 5, 4, 2 }, { 4, 9, 12, 9, 4 }, { 5, 12, 15, 12, 5 }, { 4, 9, 12, 9, 4 }, { 2, 4, 5, 4, 2 } };
    public int[,] blurMatrix2 = new int[,] { { 1, 2, 1 }, { 2, 4, 2 }, { 1, 2, 1 } };
    public int minCanny = 50;
    public int blurMatrixSum = 16;
    public int levelOfRows = 0;
    public int[] lastNfacesizes;
    public GameObject body;
    public bool showCamera = true;

    //Initialization 
    void Start()
    {
        updateFrequency = 6;
        _CamTex = new WebCamTexture(320, 240, FPS);
        RawImage rawimage = GetComponent<RawImage>();
        rawimage.texture = _CamTex;
        rawimage.material.mainTexture = _CamTex;
        _CamTex.Play();
        edgeSobel = new Texture2D(_CamTex.width, _CamTex.height);
        snap1 = new DoubleTexture(new Texture2D(_CamTex.width, _CamTex.height), new Texture2D(_CamTex.width, _CamTex.height));
        snap2 = new DoubleTexture(new Texture2D(_CamTex.width, _CamTex.height), new Texture2D(_CamTex.width, _CamTex.height));
        diff = new Texture2D(_CamTex.width, _CamTex.height);
        snap = new Texture2D(_CamTex.width, _CamTex.height);
        snap_cor = new Texture2D(_CamTex.width, _CamTex.height);
        snap_bw = new Texture2D(_CamTex.width, _CamTex.height);
        snap_prev = new Texture2D(_CamTex.width, _CamTex.height);
        body = GameObject.Find("Body");
        lastNfacesizes = new int[4];

    }


    void TakeSnapshot()
    {
        _CaptureCounter++;
        updateFrequency = 6;
        //Debug.Log(updateFrequency);

        //ReDraw the Image
        snap.SetPixels(_CamTex.GetPixels());
        snap.Apply();

        //snap = LoadPNG("D:/Man.jpg");

        //Call Face Recognition
        firstImageAverage = FaceRecognition(_CamTex.width, _CamTex.height);
        if (firstImageAverage != -1)
        {
            //Debug.Log(lastZero + " " + firstImageAverage);
            if (lastZero == 0 && firstImageAverage > 0)
            {
                rotationState = "start";
            }
            else if (lastZero > 0 && firstImageAverage == 0 && !rotationState.Equals("start"))
            {
                rotationState = "stop";
            }
            else if (lastZero == 0 && firstImageAverage == 0)
            {
                rotationState = "stay";
            }
            else
            {
                rotationState = "continue";
            }
        }
        if (_CaptureCounter % updateFrequency == 0)
        {
            //  Debug.Log(rotationState);
        }
        if (firstImageAverage == 0)
        {
            //Debug.Log(firstImageAverage);
        }
        if (_CaptureCounter % updateFrequency == 0)
        {
            snap_prev.SetPixels(_CamTex.GetPixels());
            snap_prev.Apply();
            angle = 0;
            //Debug.Log("zero");
        }


        //bool stop = false;
        if (firstImageAverage > 0)
        {
            //Debug.Log(firstImageAverage);
            if (_CaptureCounter > updateFrequency)
            {

                //Debug.Log(firstImageAverage + " " + lastImageAverage);
                angle = -(lastImageAverage - firstImageAverage) * 0.25f;
                //transform.RotateAround(new Vector3(0, 0, 12), Vector3.up, angle);
                //Debug.Log(angle);
            }
            lastImageAverage = firstImageAverage;
        }
        if (firstImageAverage != -1)
        {
            //Debug.Log(lastImageAverage + " " + firstImageAverage);
            lastZero = firstImageAverage;
        }

        //Smoothing the rotation
        // if (!stop)
        if (_CaptureCounter % updateFrequency != 0 && _CaptureCounter > FPS)
        {

            if (rotationState.Equals("start"))
            {
                float angle_cor = startRotation(_CaptureCounter % updateFrequency) * angle * rotateSpeed/10;
                body.transform.RotateAround(Vector3.up, (-1) * angle_cor / 360 * 2 * (float)System.Math.PI);
            }
            else if (rotationState.Equals("stop"))
            {
                float angle_cor = startRotation(_CaptureCounter % updateFrequency + updateFrequency) * angle * rotateSpeed/10;
                body.transform.RotateAround(Vector3.up, (-1) * angle_cor / 360 * 2 * (float)System.Math.PI);
            }
            else if (rotationState.Equals("continue"))
            {
                body.transform.RotateAround(Vector3.up, (-1) * angle * rotateSpeed / 10 / (updateFrequency - 1) / 360 * 2 * (float)System.Math.PI);
            }
            else if (rotationState.Equals("stay"))
            {

            }
            //float angle_cor = deltaAngle(_CaptureCounter % updateFrequency) * angle;
            //transform.RotateAround(new Vector3(0, 0, 12), Vector3.up, angle_cor);
            //transform.RotateAround(new Vector3(0, 0, 12), Vector3.up, angle/(updateFrequency));
        }
    }
    float deltaAngle(int stage)
    {
        float result = Mathf.Sin(Mathf.PI / (2 * updateFrequency - 1) * Mathf.Sin((2 * stage - 1) * (Mathf.PI / (2 * updateFrequency - 1))));
        return result;
    }

    float
    startRotation(int stage)
    {
        float result = Mathf.Sin(Mathf.PI / (2 * 2 * updateFrequency - 1) * Mathf.Sin((2 *
        stage - 1) * (Mathf.PI / (2 * 2 * updateFrequency - 1))));
        return result;
    }

    float stopRotation(int stage)
    {
        float result = Mathf.Sin(Mathf.PI / (2 * 2 * updateFrequency - 1) * Mathf.Sin((2 * stage - 1) * (Mathf.PI / (2 * 2 * updateFrequency - 1))));
        return result;
    }

    public static float[,] applyMatrix(Texture2D texture, int[,] matrix)
    {

        float[,] filterSnap = new float[texture.width, texture.height];
        int width = texture.width;
        int height = texture.height;
        int side = (matrix.GetLength(0) / 2);
        int matrixSize = sumIntInArr(matrix);
        for (int i = side; i < width - side; i++)
        {
            for (int j = side; j < height - side; j++)
            {
                float sum = 0;
                for (int k = -side; k <= side; k++)
                {
                    for (int q = -side; q <= side; q++)
                    {
                        sum += texture.GetPixel(i + k, j + q).grayscale * matrix[side + q, side + k];
                    }
                }
                sum /= (matrixSize == 0 ? 1 : matrixSize);

                filterSnap[i, j] = sum;
            }
        }
        return filterSnap;
    }

    public static int sumIntInArr(int[,] arr)
    {
        int sum = 0;
        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
            {
                sum += arr[i, j];
            }
        }
        return sum;
    }

    public static float sumFloatInArr(float[,] arr)
    {
        float sum = 0;
        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
            {
                sum += arr[i, j];
            }
        }
        return sum;
    }


    public Texture2D applyCanny(float[,] snapX, float[,] snapY)
    {

        int width = snapX.GetLength(0);
        int height = snapX.GetLength(1);
        float[,] angles = new float[width, height];
        Texture2D snapXY = new Texture2D(width, height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float dx = snapX[i, j];
                float dy = snapY[i, j];
                float val = Mathf.Sqrt(dx * dx + dy * dy);
                float hue = Mathf.Atan2(dy, dx) + Mathf.PI;
                float hue_angle = hue * 180 / Mathf.PI;
                angles[i, j] = hue_angle;
                snapXY.SetPixel(i, j, new Color(val, val, val));
            }
        }
        snapXY.Apply();
        Graphics.CopyTexture(snapXY, edgeSobel);
        float textureBrightness = brightness(snap);
        if (textureBrightness < 10000)
        {
            minCanny = 20;
        }
        else if (textureBrightness > 20000)
        {
            minCanny = 50;
        }

        //Debug.Log(minCanny + " " + textureBrightness);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (i == 50 && j == 50)
                {
                }
                if (!localMax(i, j, snapXY, angles[i, j]) || snapXY.GetPixel(i, j).grayscale * 255 < minCanny)
                {
                    snapXY.SetPixel(i, j, new Color(0, 0, 0));
                }
            }
        }
        snapXY.Apply();
        return snapXY;
    }

    public Texture2D applyTwoSnaps(float[,] snapX, float[,] snapY)
    {

        int width = snapX.GetLength(0);
        int height = snapX.GetLength(1);
        Texture2D snapXY = new Texture2D(width, height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float dx = snapX[i, j];
                float dy = snapY[i, j];
                float val = Mathf.Sqrt(dx * dx + dy * dy);
                float hue = Mathf.Atan2(dy, dx) + Mathf.PI;
                float hue_angle = hue * 180 / Mathf.PI;
                float[] rgb = HSVtoRGB(hue, 1, val);
                //snapXY.SetPixel(i, j, new Color(rgb[0], rgb[1], rgb[2]));
                snapXY.SetPixel(i, j, new Color(val, val, val));
            }
        }
        snapXY.Apply();
        return snapXY;
    }

    public Texture2D applyTwoSnapsWithOrientation(float[,] snapX, float[,] snapY)
    {

        int width = snapX.GetLength(0);
        int height = snapX.GetLength(1);
        Texture2D snapXY = new Texture2D(width, height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                float dx = snapX[i, j];
                float dy = snapY[i, j];
                float val = Mathf.Sqrt(dx * dx + dy * dy);
                float hue = Mathf.Atan2(dy, dx) + Mathf.PI;
                float hue_angle = hue * 180 / Mathf.PI;
                float[] rgb = HSVtoRGB(hue, 1, val);
                snapXY.SetPixel(i, j, new Color(rgb[0], rgb[1], rgb[2]));
            }
        }
        snapXY.Apply();
        return snapXY;
    }


    public bool localMax(int x, int y, Texture2D texture, float angle)
    {
        float cur = texture.GetPixel(x, y).grayscale;
        if ((angle >= 23 && angle < 68) || (angle >= 203 && angle < 248))
        {
            float left = texture.GetPixel(x - 1, y - 1).grayscale;
            float right = texture.GetPixel(x + 1, y + 1).grayscale;
            if (cur > left && cur > right)
            {
                return true;
            }
        }
        else if ((angle > 68 && angle < 113) || (angle >= 248 && angle < 293))
        {
            float left = texture.GetPixel(x, y + 1).grayscale;
            float right = texture.GetPixel(x, y - 1).grayscale;
            if (cur > left && cur > right)
            {
                return true;
            }
        }
        else if ((angle >= 113 && angle < 158) || (angle >= 293 && angle < 338))
        {
            float left = texture.GetPixel(x - 1, y + 1).grayscale;
            float right = texture.GetPixel(x + 1, y - 1).grayscale;
            if (cur > left && cur > right)
            {
                return true;
            }
        }
        else if ((angle >= 158 && angle < 203) || ((angle >= 338 && angle <= 360) || (angle >= 0 && angle < 23)))
        {
            float left = texture.GetPixel(x - 1, y).grayscale;
            float right = texture.GetPixel(x + 1, y).grayscale;
            if (cur > left && cur > right)
            {
                return true;
            }
        }
        return false;

    }

    static float maxInArr(float[,] arr)
    {
        float max = 0;

        for (int i = 0; i < arr.GetLength(0); i++)
        {
            for (int j = 0; j < arr.GetLength(1); j++)
            {
                if (max < arr[i, j])
                {
                    max = arr[i, j];
                }
            }
        }
        return max;
    }

    public static float[,] snapToArr(Texture2D snap)
    {

        float[,] res = new float[snap.width, snap.height];
        for (int i = 0; i < snap.width; i++)
        {
            for (int j = 0; j < snap.height; j++)
            {
                res[i, j] = snap.GetPixel(i, j).grayscale;
            }
        }
        return res;
    }

    public static Texture2D arrToSnap(float[,] arr)
    {

        int width = arr.GetLength(0);
        int height = arr.GetLength(1);
        Texture2D texture = new Texture2D(width, height);
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                texture.SetPixel(i, j, new Color(arr[i, j], arr[i, j], arr[i, j]));
            }
        }
        texture.Apply();
        return texture;
    }


    void OnGUI()
    {


        if (showCamera)
        {
            GUI.DrawTexture(new Rect(0, 0, 320, 240), snap);
            //GUI.DrawTexture(new Rect(0, 240, 320, 240), snap1.Texture);
            //GUI.DrawTexture(new Rect(320, 240, 320, 240), snap2.Texture);
            //GUI.DrawTexture(new Rect(0, 480, 320, 240), snap1.Edge);
            // GUI.DrawTexture(new Rect(320, 480, 320, 240), snap2.Edge);
        }
        
        /*
        try
        {
            if (snap_cor.GetPixel(0, 0) == Color.red)
            {
                GUI.DrawTexture(new Rect(0, 0, 320, 240), snap_cor);
            }

        }
        catch (UnassignedReferenceException e) { }
        */
        /*
            for (int i = 0; i < strArr.Length; i++ )
                 GUI.Label(new Rect(0, i*10, 200, 10), strArr[i]);

            */
    }

    public Texture2D toGrayscale(Texture2D texture)
    {
        for (int i = 0; i < texture.width; i++)
        {
            for (int j = 0; j < texture.height; j++)
            {
                float color = texture.GetPixel(i, j).grayscale;
                texture.SetPixel(i, j, new Color(color, color, color));
            }
        }
        return texture;

    }

    public static float brightness(Texture2D texture)
    {
        float res = sumFloatInArr(snapToArr(texture));
        return res;
    }

    //public static Texture2D LoadPNG(string filePath) 
    // { 

    // Texture2D tex = null; 
    // byte[] fileData; 
    // 
    // if (File.Exists(filePath)) 
    // { 
    // fileData = File.ReadAllBytes(filePath); 
    // tex = new Texture2D(1024, 576); 
    // tex.LoadImage(fileData); //..this will auto-resize the texture dimensions. 
    // } 
    // return tex; 
    //} 

    DoubleTexture newSnap()
    {
        snap.SetPixels(_CamTex.GetPixels());
        snap = toGrayscale(snap);
        snap.Apply();
        Texture2D snapDummy = new Texture2D(snap.width, snap.height);
        Graphics.CopyTexture(snap, snapDummy);


        blurSnap = arrToSnap(applyMatrix(snapDummy, blurMatrix));
        blurSnap.Apply();
        //Debug.Log(blurSnap.GetPixel(160, 120));
        Graphics.CopyTexture(blurSnap, snapDummy);


        float[,] appliedXSnap = applyMatrix(snapDummy, edgeXMatrix);
        edgeXSnap = arrToSnap(appliedXSnap);
        Graphics.CopyTexture(blurSnap, snapDummy);

        float[,] appliedYSnap = applyMatrix(snapDummy, edgeYMatrix);
        edgeYSnap = arrToSnap(appliedYSnap);
        edgeSnap = applyCanny(appliedXSnap, appliedYSnap);
        //edgeSnap = appliedYSnap;
        edgeSnap.Apply();
        DoubleTexture res = new DoubleTexture(snapDummy, edgeSnap);

        edgeColorSnap = applyTwoSnapsWithOrientation(appliedXSnap, appliedYSnap);
        edgeColorSnap.Apply();


        //float[] color = HSVtoRGB(5, 1, 5);
        //Debug.Log(color[0]*255 + " " + color[1] * 255 + " " + color[2] * 255);
        return res;
    }

    Texture2D difference(DoubleTexture prev, DoubleTexture next)
    {
        Texture2D res = new Texture2D(prev.Edge.width, prev.Edge.height);
        int margin = prev.Texture.width - prev.Edge.width;
        for (int i = 2; i < prev.Texture.width - 2; i++)
        {
            for (int j = 2; j < prev.Texture.height - 2; j++)
            {
                if (Mathf.Abs(prev.Texture.GetPixel(i, j).grayscale - next.Texture.GetPixel(i, j).grayscale) > 0.07f)
                {
                    res.SetPixel(i, j, next.Edge.GetPixel(i, j));
                }
                else
                {
                    res.SetPixel(i, j, Color.black);
                }
            }
        }
        res.Apply();
        return res;
    }

    Texture2D difference(Texture2D prev, Texture2D next)
    {
        Texture2D res = new Texture2D(prev.width, prev.height);
        for (int i = 2; i < prev.width - 2; i++)
        {
            for (int j = 2; j < prev.height - 2; j++)
            {
                if (Mathf.Abs(prev.GetPixel(i, j).grayscale - next.GetPixel(i, j).grayscale) > 0.12f)
                {
                    res.SetPixel(i, j, next.GetPixel(i, j));
                }
                else
                {
                    res.SetPixel(i, j, Color.black);
                }
            }
        }
        res.Apply();
        return res;
    }

    public static bool checkWhiteClose(int a, int b, Texture2D prev, Texture2D next)
    {
        for (int i = 3; i <= 3; i++)
        {
            for (int j = 3; j <= 3; j++)
            {
                if (Mathf.Abs(prev.GetPixel(a + i, b + j).grayscale - next.GetPixel(a, b).grayscale) > 0.12f || Mathf.Abs(next.GetPixel(a + i, b + j).grayscale - prev.GetPixel(a, b).grayscale) > 0.12f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static float[] HSVtoRGB(float h, float s, float v)
    {
        h = h / (2 * Mathf.PI) * 6;

        float c = s * v;
        float x = c * (1 - Mathf.Abs((h % 2) - 1));
        float r, g, b;
        if (h < 1)
        {
            r = c;
            g = x;
            b = 0;
        }
        else if (h < 2)
        {
            r = x;
            g = c;
            b = 0;
        }
        else if (h < 3)
        {
            r = 0;
            g = c;
            b = x;
        }
        else if (h < 4)
        {
            r = 0;
            g = x;
            b = c;
        }
        else if (h < 5)
        {
            r = x;
            g = 0;
            b = c;
        }
        else
        {
            r = c;
            g = 0;
            b = x;
        }

        float m = v - c;

        r += m;
        g += m;
        b += m;

        return new float[] { r, g, b };
    }

    void Update()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Input.GetMouseButton(0))
        {
            //Vector3 position = transform.position;
            // Debug.Log ("camera " + position);
            //ModelMover.model.Rotate (new Vector3 (0, Input.GetAxis("Mouse X") * rotateSpeed, 0));
            //Debug.Log ("cylinder" + CameraMover.camera.position);
            //transform.RotateAround (new Vector3 (0, 0, 8), Vector3.up, Input.GetAxis ("Mouse X") * rotateSpeed);
            body.transform.RotateAround(Vector3.up, ((-1) * Input.GetAxis("Mouse X") * rotateSpeed) / 360 * 2 * (float)System.Math.PI);//this sht sometimes looks дерганым я хз как фикстить
            body.transform.RotateAround(Vector3.right, (Input.GetAxis("Mouse Y") * rotateSpeed) / 360 * 2 * (float)System.Math.PI);
            if (Input.GetKey(KeyCode.LeftControl))
            {
                body.transform.RotateAround(Vector3.forward, (Input.GetAxis("Mouse Y") * rotateSpeed) / 360 * 2 * (float)System.Math.PI);
            }
            //transform.RotateAround( new Vector3(1, position.y, -(position.z)/position.x), Vector3.right, Input.GetAxis("Mouse Y") * rotateSpeed );

        }
        else if (scroll != 0 && (((Vector3.Distance(GetComponent<Camera>().transform.position, body.transform.position) > 15 && scroll > 0)) || (scroll < 0 && Vector3.Distance(GetComponent<Camera>().transform.position, body.transform.position) < 200)))
        {
            //GetComponent<Camera> ().transform.LookAt (body.transform.position);
            Debug.Log(GetComponent<Camera>().transform.position);
            float xpos = GetComponent<Camera>().transform.position.x;
            float ypos = GetComponent<Camera>().transform.position.y;
            float zpos = GetComponent<Camera>().transform.position.z;
            //GetComponent<Camera> ().transform.position = new Vector3( xpos + scroll, ypos/xpos * (xpos + scroll), zpos/xpos * (xpos + scroll)) ;
            Vector3 translation = new Vector3((body.transform.position.x - xpos) / 10 * scroll, (body.transform.position.y - 5 - ypos) / 10 * scroll, (body.transform.position.z - zpos) / 10 * scroll);
            GetComponent<Camera>().transform.Translate(translation);
            /* 
                GetComponent<Camera> ().transform.position = new Vector3(  xpos  - 1.4f/5f * System.Math.Sign(scroll),
                ypos + 2.2f/5f * System.Math.Sign(scroll), zpos + 17.1f/5f * System.Math.Sign(scroll)) ;
                */
            Debug.Log(GetComponent<Camera>().transform.position);
        }
        else if (Input.GetMouseButton(1))
        {
            body.transform.position += new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0);
        }
        else if (Input.GetKeyDown(KeyCode.R))
        { // do not do it if you want the code working
            GetComponent<Camera>().transform.LookAt(body.transform.position);
            Debug.Log(GetComponent<Camera>().transform.rotation);
            //body.transform.RotateAround (Vector3.up, (float)System.Math.PI);
        }
        else
        {

            TakeSnapshot();
        }
        if (Input.GetKeyDown("space"))
        {
            showCamera = !showCamera;
        }


        if (_CaptureCounter % 100 == 99)
        {

            /*
            float summ = 0;
            for (int i = 0; i < _CamTex.width; i++) {
                for (int j = 0; j < _CamTex.height; j++) {
                    summ += snap.GetPixel (i, j).grayscale;
                }
            }
            //Debug.Log (summ / (_CamTex.width * _CamTex.height) + " average");
        */
            //  Debug.Log (snap.GetPixel (56, 176).grayscale + " 56, 176");
        }
        //Call Snapshot

        //Exit by pressing ESC button
        if (Input.GetKeyDown(KeyCode.Escape))
            Application.Quit();
    }
}
