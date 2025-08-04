using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;

public class SelectionBox : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform selectionBoxUI;
    public Canvas canvas;
    public RectTransform targetImage; // �ؼЪ� Image RectTransform
    public Image Image;

    private Camera uiCamera;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool isDragging = false;

    public bool check = false;
    public TMP_Text SeeAns;

    public StableDiffusionRegionPrompt stableDiffusionRegionPrompt;
    List<string> result;
    public List<string> B = new List<string>();

    private bool first = true;
    // ����ت��|�Ө����y��
    public Vector2 TopLeft { get; private set; }
    public Vector2 TopRight { get; private set; }
    public Vector2 BottomLeft { get; private set; }
    public Vector2 BottomRight { get; private set; }


    string[] AllQ = new string[]
    {
        @"
            {�D�D=Tranquil lakeside ecology}
            {Background=0=0=1=1=serene lake, calm water, dense forest, clear sky=0}
            {Foreground=0.15=0.55=0.3=0.4=1 deer, graceful, brown fur, drinking water, alert expression=0}
            {Environment=0.1=0.5=0.4=0.45=lush green grass, smooth pebbles, reflections in water=0}
            {Foreground=0.6=0.45=0.35=0.5=1 brown bear, large, shaggy fur, standing on hind legs, sniffing the air=0}
            {Environment=0.55=0.4=0.45=0.6=tall pine trees, fallen leaves, rocky outcrop=0}
            {Foreground=0.4=0.6=0.3=0.4=1 white swan, elegant, long neck, gliding smoothly, on the water=0}
            {Environment=0.35=0.55=0.4=0.5=lily pads, ripples in water, sunlit surface=0} 
        ",
        @"
            {�D�D=A forest full of life}
            {Background=0=0=1=1=A vast, serene forest clearing, dappled sunlight filtering through dense canopy, ancient trees with moss-covered bark, a gentle stream winding through vibrant green undergrowth=0}
            {Foreground=0.2=0.4=0.5=0.3=1 majestic stag, large antlers, calm demeanor, standing alert, in a clearing=0}
            {Enviroment=0.15=0.35=0.6=0.4=A patch of lush ferns and wild flowers, dew drops clinging to petals, soft moss covering the forest floor=0}
            {Foreground=0.6=0.5=0.3=0.3=1 curious fox, reddish-brown fur, pointed ears, sniffing the air, near a fallen log=0}
            {Enviroment=0.55=0.45=0.4=0.4=A weathered fallen log covered in mushrooms and lichens, scattered autumn leaves on the ground=0}
            {Foreground=0.8=0.6=0.3=0.2=1 small rabbit, soft grey fur, twitching nose, nibbling on grass, partially hidden behind a bush=0}
            {Enviroment=0.75=0.55=0.4=0.3=A cluster of low-lying bushes with dark green leaves and a patch of soft grass=0} 
        ",
        @"
            {�D�D=Tropical jungle ecology}
            {Background=0=0=1=1=a lush, dense jungle, vibrant green foliage, ancient trees, dappled sunlight, hanging vines, misty atmosphere=0}
            {Foreground=0.2=0.4=0.5=0.4=1 jaguar, powerful build, spotted fur, alert expression, stalking through undergrowth=0}
            {Enviroment=0.15=0.35=0.6=0.5=dense jungle floor, thick moss, tangled roots, fallen leaves, ferns=0}
            {Foreground=0.6=0.5=0.4=0.4=1 toucan, colorful plumage, large beak, perched on a branch=0}
            {Enviroment=0.55=0.45=0.5=0.5=thick jungle branches, large leaves, mossy bark, vines=0}
            {Foreground=0.8=0.6=0.3=0.4=1 colorful butterfly, delicate wings, intricate patterns, fluttering near flowers=0}
            {Enviroment=0.75=0.55=0.4=0.5=exotic flowers, bright petals, various shapes, dew drops=0} 
        ",
        @"
            {�D�D=Undersea coral reef ecology}
            {Background=0=0=1=1=a vibrant underwater coral reef, clear turquoise water, shafts of sunlight filtering down, colorful anemones, sandy seabed=0}
            {Foreground=0.35=0.5=0.45=0.3=1green sea turtle, large, ancient, patterned shell, gracefully swimming, flippers extended=0}
            {Enviroment=0.25=0.4=0.6=0.5=swaying kelp forests, delicate sea plants, rocky outcrops, dappled light patterns=0}
            {Foreground=0.7=0.6=0.35=0.4=1clownfish, small, bright orange and white stripes, nestled among anemone tentacles=0}
            {Enviroment=0.65=0.55=0.45=0.4=sea anemones, colorful, flowing tentacles, providing shelter=0}
            {Foreground=0.1=0.7=0.25=0.4=1seahorse, small, coiled tail, camouflaged against coral=0}
            {Enviroment=0.05=0.7=0.35=0.4=a cluster of vibrant purple coral, textured surface, small crevices=0}
        ",
        @"
            {�D�D=Snow Mountain Ecology}
            {Background=0=0=1=1=a serene mountain landscape, snow-capped peaks, clear blue sky, pine forests, a winding river=0}
            {Foreground=0.45=0.15=0.4=0.3=1 eagle, majestic, soaring, sharp eyes, feathered wings=0}
            {Enviroment=0.35=0.1=0.5=0.35=rocky cliff face, scattered pine trees, weathered stone=0}
            {Foreground=0.2=0.5=0.3=0.2=1 deer, graceful, standing, alert, brown fur=0}
            {Enviroment=0.1=0.5=0.4=0.3=lush green meadow, wildflowers, tall grass=0}
            {Foreground=0.7=0.45=0.25=0.2=1 wolf, powerful, resting, looking to the side, grey fur=0}
            {Enviroment=0.65=0.45=0.4=0.3=dense forest undergrowth, fallen leaves, mossy ground=0}
        ",
    };
    void Start()
    {
        if (Image != null && check == false)
        {
            Sprite[] Hint = Resources.LoadAll<Sprite>("multi�D�w");

            //�q�D�w���D
            int rand = UnityEngine.Random.Range(0, 5);
            List<string> keywords = stableDiffusionRegionPrompt.gameController.ExtractContentsSeparatedByDash(AllQ[rand]);
            Image.sprite = System.Array.Find(
                        Hint,
                        sprite => sprite.name.Contains("multi" + rand)
                    );
            stableDiffusionRegionPrompt.gameController.usedMainPrompt += ", " + keywords[1];
            Debug.Log("�o���D�ت��D�D:" + keywords[1]);
            for (int i = 0; i < keywords.Count / 7; i++)
            {
                stableDiffusionRegionPrompt.InputRegion(keywords[i * 7 + 2], float.Parse(keywords[i * 7 + 1 + 2]), float.Parse(keywords[i * 7 + 2 + 2]), float.Parse(keywords[i * 7 + 3 + 2]), float.Parse(keywords[i * 7 + 4 + 2]), keywords[i * 7 + 5 + 2], keywords[i * 7 + 6 + 2]);
                //string BlendMode, float X, float Y, float W, float H,string Prompt,string Neg_Prompt
            }
        }
        
        


        // �T�O����ض}�l�ɬO���ê�
        if (selectionBoxUI != null)
            selectionBoxUI.gameObject.SetActive(false);

        // ���UI Camera
        if (canvas != null)
        {
            uiCamera = canvas.worldCamera;
        }

        // �p�G�S���]�wCanvas�A���զ۰ʬd��
        if (canvas == null && selectionBoxUI != null)
        {
            canvas = selectionBoxUI.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                uiCamera = canvas.worldCamera;
            }
        }
    }

    void Update()
    {
        HandleInput();

        if (isDragging)
        {
            UpdateSelectionBox();
        }
    }

    void HandleInput()
    {
        // �}�l�즲
        if (Input.GetMouseButtonDown(0))
        {
            // �ˬd�O�_�I���b targetImage ��
            if (IsPointInTargetImage(Input.mousePosition))
            {
                StartSelection();
            }
        }

        // �����즲
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndSelection();
        }
    }

    // �ˬd�I����m�O�_�b targetImage ��
    bool IsPointInTargetImage(Vector2 screenPoint)
    {
        if (targetImage == null) return false;

        // �N�ù��y���ഫ�� targetImage �����a�y��
        Vector2 localPoint;
        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetImage,
            screenPoint,
            uiCamera,
            out localPoint
        );

        // �ˬd�I�O�_�b RectTransform ���d��
        if (isInside)
        {
            Rect rect = targetImage.rect;
            return rect.Contains(localPoint);
        }

        return false;
    }

    void StartSelection()
    {
        startPosition = Input.mousePosition;
        endPosition = startPosition;
        isDragging = true;

        // ��ܿ����
        if (selectionBoxUI != null)
        {
            selectionBoxUI.gameObject.SetActive(true);

            // �ഫ�ƹ��y�Ш�Canvas�y��
            Vector2 startCanvasPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                selectionBoxUI.parent as RectTransform,
                startPosition,
                uiCamera,
                out startCanvasPos
            );

            selectionBoxUI.localPosition = startCanvasPos;
            selectionBoxUI.sizeDelta = Vector2.zero;
        }
    }

    void UpdateSelectionBox()
    {
        endPosition = Input.mousePosition;

        // ����즲�d��b targetImage ��
        endPosition = ClampToTargetImage(endPosition);

        if (selectionBoxUI != null)
        {
            // �ഫ�ƹ��y�Ш�Canvas�y��
            Vector2 startCanvasPos;
            Vector2 endCanvasPos;

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                selectionBoxUI.parent as RectTransform,
                startPosition,
                uiCamera,
                out startCanvasPos
            );

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                selectionBoxUI.parent as RectTransform,
                endPosition,
                uiCamera,
                out endCanvasPos
            );

            // �p�����ت������I�M�j�p
            Vector2 boxCenter = (startCanvasPos + endCanvasPos) / 2;
            Vector2 boxSize = new Vector2(
                Mathf.Abs(startCanvasPos.x - endCanvasPos.x),
                Mathf.Abs(startCanvasPos.y - endCanvasPos.y)
            );

            selectionBoxUI.localPosition = boxCenter;
            selectionBoxUI.sizeDelta = boxSize;
        }

        // ��s�|�Ө����y��
        UpdateCornerPositions();
    }

    // ����y�Цb targetImage �d��
    Vector2 ClampToTargetImage(Vector2 screenPoint)
    {
        if (targetImage == null) return screenPoint;

        // ��� targetImage ���@�ɮy�����
        Vector3[] imageCorners = new Vector3[4];
        targetImage.GetWorldCorners(imageCorners);

        // �ഫ���ù��y��
        Vector2 imageBottomLeft = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[0]);
        Vector2 imageTopRight = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[2]);

        // ����b Image �d��
        float clampedX = Mathf.Clamp(screenPoint.x, imageBottomLeft.x, imageTopRight.x);
        float clampedY = Mathf.Clamp(screenPoint.y, imageBottomLeft.y, imageTopRight.y);

        return new Vector2(clampedX, clampedY);
    }

    void UpdateCornerPositions()
    {
        float minX = Mathf.Min(startPosition.x, endPosition.x);
        float maxX = Mathf.Max(startPosition.x, endPosition.x);
        float minY = Mathf.Min(startPosition.y, endPosition.y);
        float maxY = Mathf.Max(startPosition.y, endPosition.y);

        TopLeft = new Vector2(minX, maxY);
        TopRight = new Vector2(maxX, maxY);
        BottomLeft = new Vector2(minX, minY);
        BottomRight = new Vector2(maxX, minY);
    }

    void EndSelection()
    {
        isDragging = false;

        // ���ÿ����
        if (selectionBoxUI != null)
            //selectionBoxUI.gameObject.SetActive(false);

            // �p��ÿ�X�b Image �W���ʤ����m
            if (targetImage != null)
            {
                CalculateImagePercentagePositions();
                first = false;
            }
    }

    // �p�����إ|�Ө��b Image �W���ʤ����m
    void CalculateImagePercentagePositions()
    {
        if (targetImage == null) return;

        // ��� Image ���@�ɮy�����
        Vector3[] imageCorners = new Vector3[4];
        targetImage.GetWorldCorners(imageCorners);

        // �ഫ���ù��y��
        Vector2 imageBottomLeft = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[0]);
        Vector2 imageTopRight = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[2]);

        // �p�� Image ���ù��y�нd��
        float imageLeft = imageBottomLeft.x;
        float imageRight = imageTopRight.x;
        float imageBottom = imageBottomLeft.y;
        float imageTop = imageTopRight.y;

        float imageWidth = imageRight - imageLeft;
        float imageHeight = imageTop - imageBottom;

        // �p��U�����ʤ����m
        Vector2 topLeftPercent = CalculatePercentagePosition(TopLeft, imageLeft, imageBottom, imageWidth, imageHeight);
        Vector2 topRightPercent = CalculatePercentagePosition(TopRight, imageLeft, imageBottom, imageWidth, imageHeight);
        Vector2 bottomLeftPercent = CalculatePercentagePosition(BottomLeft, imageLeft, imageBottom, imageWidth, imageHeight);
        Vector2 bottomRightPercent = CalculatePercentagePosition(BottomRight, imageLeft, imageBottom, imageWidth, imageHeight);

        float x = Mathf.Round((Mathf.Min(topLeftPercent.x, bottomLeftPercent.x) * 0.01f) * 100f) / 100f;
        float y = Mathf.Round((Mathf.Min(100 - topLeftPercent.y, 100 - bottomLeftPercent.y) * 0.01f) * 100f) / 100f;
        float w = Mathf.Round((Mathf.Abs(topLeftPercent.x - topRightPercent.x) * 0.01f) * 100f) / 100f;
        float h = Mathf.Round((Mathf.Abs(topLeftPercent.y - bottomLeftPercent.y) * 0.01f) * 100f) / 100f;

        result = new List<string>();

        result.AddRange(stableDiffusionRegionPrompt.AllRegions[0].prompt.Split(','));

        for (int i = 1; i < stableDiffusionRegionPrompt.AllRegions.Count; i++)
        {
            float Xt1 = Mathf.Max(x, stableDiffusionRegionPrompt.AllRegions[i].x);
            float Xt2 = Mathf.Min(x + w, stableDiffusionRegionPrompt.AllRegions[i].x + stableDiffusionRegionPrompt.AllRegions[i].w);
            float X = Mathf.Max(0, Xt2 - Xt1);
            float Yt1 = Mathf.Max(y, stableDiffusionRegionPrompt.AllRegions[i].y);
            float Yt2 = Mathf.Min(y + h, stableDiffusionRegionPrompt.AllRegions[i].y + stableDiffusionRegionPrompt.AllRegions[i].h);
            float Y = Mathf.Max(0, Yt2 - Yt1);
            float area = X * Y;

            if ((area >= stableDiffusionRegionPrompt.AllRegions[i].w * stableDiffusionRegionPrompt.AllRegions[i].h * 0.4) && (area >= w * h * 0.5))
            {
                result.AddRange(stableDiffusionRegionPrompt.AllRegions[i].prompt.Split(','));
            }
        }
        Debug.Log("c : "+string.Join(", ", result));
        if(check == true)
        {
            SeeAns.text = result[0];
            stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(11);
        }
    }

    // �p�����I�b Image �W���ʤ����m
    Vector2 CalculatePercentagePosition(Vector2 screenPoint, float imageLeft, float imageBottom, float imageWidth, float imageHeight)
    {
        // �p��۹�� Image ���U������m
        float relativeX = screenPoint.x - imageLeft;
        float relativeY = screenPoint.y - imageBottom;

        // �ഫ���ʤ��� (0-100)
        float percentX = Mathf.Clamp01(relativeX / imageWidth) * 100;
        float percentY = Mathf.Clamp01(relativeY / imageHeight) * 100;

        return new Vector2(percentX, percentY);
    }

    // ���}��k�G�������ئb���w Image �W���ʤ����m
    public Dictionary<string, Vector2> GetSelectionPercentageOnImage(RectTransform image)
    {
        if (image == null || !isDragging) return null;

        // ��� Image ���@�ɮy�����
        Vector3[] imageCorners = new Vector3[4];
        image.GetWorldCorners(imageCorners);

        // �ഫ���ù��y��
        Vector2 imageBottomLeft = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[0]);
        Vector2 imageTopRight = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[2]);

        float imageLeft = imageBottomLeft.x;
        float imageBottom = imageBottomLeft.y;
        float imageWidth = imageTopRight.x - imageLeft;
        float imageHeight = imageTopRight.y - imageBottom;

        var result = new Dictionary<string, Vector2>();
        result["TopLeft"] = CalculatePercentagePosition(TopLeft, imageLeft, imageBottom, imageWidth, imageHeight);
        result["TopRight"] = CalculatePercentagePosition(TopRight, imageLeft, imageBottom, imageWidth, imageHeight);
        result["BottomLeft"] = CalculatePercentagePosition(BottomLeft, imageLeft, imageBottom, imageWidth, imageHeight);
        result["BottomRight"] = CalculatePercentagePosition(BottomRight, imageLeft, imageBottom, imageWidth, imageHeight);

        return result;
    }

    // ���}��k�G�ˬd�O�_���b�즲
    public bool IsDragging()
    {
        return isDragging;
    }

    // ���}��k�G�������ت��x�ΰϰ�
    public Rect GetSelectionRect()
    {
        if (!isDragging) return new Rect();

        float minX = Mathf.Min(startPosition.x, endPosition.x);
        float minY = Mathf.Min(startPosition.y, endPosition.y);
        float width = Mathf.Abs(startPosition.x - endPosition.x);
        float height = Mathf.Abs(startPosition.y - endPosition.y);

        return new Rect(minX, minY, width, height);
    }

    public void CheckAnswerForSelect(InputField inputField)
    {
        if(inputField.text != "" && !first)
        {
            string LLMPrompt = $@"
���U�ӧڷ|���A�T���r�A���O��1.a 2.b�A�䤤b�|�Ѧh�Ӥ��A��([])�]�����զ��A�_�h�p�Ga�����Mb�������Y�ӵ��N��۪�A�p�Ga�����Mb�������Y�ӵ��N��۪�A�A�ݭn�^�ǡu�^�����T�v�ç�b���N��̬۪񪺤@�ӵ��]�^�ǥX�ӡA�p�Ga�S���Mb��������N��۪�A�A�ݭn�^�ǡu�^�����~�v�A�P�_�зǤ��Τ��Y��A�u�n�D��@�˴N���A�N��O���P���y���u�n�N��۪�N��A�ݭn������]

a:{inputField.text}

b:{string.Join(", ", result.Select(x => $"[{x}]"))}

��X��: ���G:{{�^�����T/�^�����~}} �N��̬۪񪺤@�ӵ�:{{(�Y���G���O�^�����T�h�d��)}} �бN���G�M�N��۪񪺦r�ȥ�����i{{}}�̭�
";
            Debug.Log(LLMPrompt);
            StartCoroutine(stableDiffusionRegionPrompt.geminiAPI.SendRequest(LLMPrompt, (r) => {
                MatchCollection matches = Regex.Matches(r, @"\{(.*?)\}");
                List<string> results = new List<string>();

                foreach (Match match in matches)
                {
                    results.Add(match.Groups[1].Value); // �u�� {} �������e�A���t�j�A��
                }
                if (results[0] == "�^�����T")
                {
                    results[1] = Regex.Replace(results[1], @"[\[\]\{\}\(\)]", "");
                    results[1] = Regex.Replace(results[1], @"\d", "").Trim();
                    if (B.Contains(results[1]))
                    {
                        stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(3);
                    }
                    else
                    {
                        B.Add(results[1]);
                        stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(2);
                    }
                }
                else
                {
                    stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(4);
                }
                inputField.text = "";
            }));
        }
        else if(first)
        {
            stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(5);
        }
        else
        {
            stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(6);
        }
    }
}