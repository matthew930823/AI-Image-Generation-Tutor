using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectionBox : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform selectionBoxUI;
    public Canvas canvas;
    public RectTransform targetImage; // �n�˴��� Image RectTransform

    private Camera uiCamera;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool isDragging = false;

    public StableDiffusionRegionPrompt stableDiffusionRegionPrompt;

    // ����ت��|�Ө����y��
    public Vector2 TopLeft { get; private set; }
    public Vector2 TopRight { get; private set; }
    public Vector2 BottomLeft { get; private set; }
    public Vector2 BottomRight { get; private set; }

    void Start()
    {
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
        // �}�l���
        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }

        // �������
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndSelection();
        }
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

        // ��s�|�Ө������y��
        UpdateCornerPositions();
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

        //// ��X���e�y�аT��
        //Debug.Log($"����ؤ��e�y��:");
        //Debug.Log($"���W��: {TopLeft}");
        //Debug.Log($"�k�W��: {TopRight}");
        //Debug.Log($"���U��: {BottomLeft}");
        //Debug.Log($"�k�U��: {BottomRight}");

        // �p��ÿ�X�b Image �W���ʤ����m
        if (targetImage != null)
        {
            CalculateImagePercentagePositions();
        }
    }

    // �p�����إ|�Ө����b Image �W���ʤ����m
    void CalculateImagePercentagePositions()
    {
        if (targetImage == null) return;

        // ��� Image ���@�ɮy�����
        Vector3[] imageCorners = new Vector3[4];
        targetImage.GetWorldCorners(imageCorners);

        // �ഫ���̹��y��
        Vector2 imageBottomLeft = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[0]);
        Vector2 imageTopRight = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[2]);

        // �p�� Image ���̹��y�нd��
        float imageLeft = imageBottomLeft.x;
        float imageRight = imageTopRight.x;
        float imageBottom = imageBottomLeft.y;
        float imageTop = imageTopRight.y;

        float imageWidth = imageRight - imageLeft;
        float imageHeight = imageTop - imageBottom;

        // �p��U�������ʤ����m
        Vector2 topLeftPercent = CalculatePercentagePosition(TopLeft, imageLeft, imageBottom, imageWidth, imageHeight);
        Vector2 topRightPercent = CalculatePercentagePosition(TopRight, imageLeft, imageBottom, imageWidth, imageHeight);
        Vector2 bottomLeftPercent = CalculatePercentagePosition(BottomLeft, imageLeft, imageBottom, imageWidth, imageHeight);
        Vector2 bottomRightPercent = CalculatePercentagePosition(BottomRight, imageLeft, imageBottom, imageWidth, imageHeight);

        //Debug.Log("=== ����ئb Image �W���ʤ����m ===");
        //Debug.Log($"���W��: X={topLeftPercent.x:F2}%, Y={topLeftPercent.y:F2}%");
        //Debug.Log($"�k�W��: X={topRightPercent.x:F2}%, Y={topRightPercent.y:F2}%");
        //Debug.Log($"���U��: X={bottomLeftPercent.x:F2}%, Y={bottomLeftPercent.y:F2}%");
        //Debug.Log($"�k�U��: X={bottomRightPercent.x:F2}%, Y={bottomRightPercent.y:F2}%");

        float x = Mathf.Round((Mathf.Min(topLeftPercent.x, bottomLeftPercent.x) * 0.01f) * 100f) / 100f;
        float y = Mathf.Round((Mathf.Min(100 - topLeftPercent.y, 100 - bottomLeftPercent.y) * 0.01f) * 100f) / 100f;
        float w = Mathf.Round((Mathf.Abs(topLeftPercent.x- topRightPercent.x) * 0.01f) * 100f) / 100f;
        float h = Mathf.Round((Mathf.Abs(topLeftPercent.y-bottomLeftPercent.y) * 0.01f) * 100f) / 100f;

        Debug.Log("x:" + x + "\ny:" + y + "\nw:" + w + "\nh:" + h);
        //Debug.Log(stableDiffusionRegionPrompt.AllRegions.Count);
        for(int i=0;i< stableDiffusionRegionPrompt.AllRegions.Count; i++)
        {
            float Xt1 = Mathf.Max(x, stableDiffusionRegionPrompt.AllRegions[i].x);
            float Xt2 = Mathf.Min(x+w, stableDiffusionRegionPrompt.AllRegions[i].x+ stableDiffusionRegionPrompt.AllRegions[i].w);
            float X = Mathf.Max(0, Xt2-Xt1);
            float Yt1 = Mathf.Max(y, stableDiffusionRegionPrompt.AllRegions[i].y);
            float Yt2 = Mathf.Min(y + h, stableDiffusionRegionPrompt.AllRegions[i].y + stableDiffusionRegionPrompt.AllRegions[i].h);
            float Y = Mathf.Max(0, Yt2 - Yt1);
            float area = x * y;
            if((area >= stableDiffusionRegionPrompt.AllRegions[i].w * stableDiffusionRegionPrompt.AllRegions[i].h * 0.7) &&(area >= w * h * 0.6))
            {
                string[] result = stableDiffusionRegionPrompt.AllRegions[i].prompt.Split(',');
                print(result);
            }
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

        // �ഫ���̹��y��
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

    // ���}��k�G�ˬd�O�_���b���
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
}