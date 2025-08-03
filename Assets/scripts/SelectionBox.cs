using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SelectionBox : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform selectionBoxUI;
    public Canvas canvas;
    public RectTransform targetImage; // 要檢測的 Image RectTransform

    private Camera uiCamera;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool isDragging = false;

    public StableDiffusionRegionPrompt stableDiffusionRegionPrompt;

    // 選取框的四個角落座標
    public Vector2 TopLeft { get; private set; }
    public Vector2 TopRight { get; private set; }
    public Vector2 BottomLeft { get; private set; }
    public Vector2 BottomRight { get; private set; }

    void Start()
    {
        // 確保選取框開始時是隱藏的
        if (selectionBoxUI != null)
            selectionBoxUI.gameObject.SetActive(false);

        // 獲取UI Camera
        if (canvas != null)
        {
            uiCamera = canvas.worldCamera;
        }

        // 如果沒有設定Canvas，嘗試自動查找
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
        // 開始拖拽
        if (Input.GetMouseButtonDown(0))
        {
            StartSelection();
        }

        // 結束拖拽
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

        // 顯示選取框
        if (selectionBoxUI != null)
        {
            selectionBoxUI.gameObject.SetActive(true);

            // 轉換滑鼠座標到Canvas座標
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
            // 轉換滑鼠座標到Canvas座標
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

            // 計算選取框的中心點和大小
            Vector2 boxCenter = (startCanvasPos + endCanvasPos) / 2;
            Vector2 boxSize = new Vector2(
                Mathf.Abs(startCanvasPos.x - endCanvasPos.x),
                Mathf.Abs(startCanvasPos.y - endCanvasPos.y)
            );

            selectionBoxUI.localPosition = boxCenter;
            selectionBoxUI.sizeDelta = boxSize;
        }

        // 更新四個角落的座標
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

        // 隱藏選取框
        if (selectionBoxUI != null)
            //selectionBoxUI.gameObject.SetActive(false);

        //// 輸出內容座標訊息
        //Debug.Log($"選取框內容座標:");
        //Debug.Log($"左上角: {TopLeft}");
        //Debug.Log($"右上角: {TopRight}");
        //Debug.Log($"左下角: {BottomLeft}");
        //Debug.Log($"右下角: {BottomRight}");

        // 計算並輸出在 Image 上的百分比位置
        if (targetImage != null)
        {
            CalculateImagePercentagePositions();
        }
    }

    // 計算選取框四個角落在 Image 上的百分比位置
    void CalculateImagePercentagePositions()
    {
        if (targetImage == null) return;

        // 獲取 Image 的世界座標邊界
        Vector3[] imageCorners = new Vector3[4];
        targetImage.GetWorldCorners(imageCorners);

        // 轉換為屏幕座標
        Vector2 imageBottomLeft = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[0]);
        Vector2 imageTopRight = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[2]);

        // 計算 Image 的屏幕座標範圍
        float imageLeft = imageBottomLeft.x;
        float imageRight = imageTopRight.x;
        float imageBottom = imageBottomLeft.y;
        float imageTop = imageTopRight.y;

        float imageWidth = imageRight - imageLeft;
        float imageHeight = imageTop - imageBottom;

        // 計算各角落的百分比位置
        Vector2 topLeftPercent = CalculatePercentagePosition(TopLeft, imageLeft, imageBottom, imageWidth, imageHeight);
        Vector2 topRightPercent = CalculatePercentagePosition(TopRight, imageLeft, imageBottom, imageWidth, imageHeight);
        Vector2 bottomLeftPercent = CalculatePercentagePosition(BottomLeft, imageLeft, imageBottom, imageWidth, imageHeight);
        Vector2 bottomRightPercent = CalculatePercentagePosition(BottomRight, imageLeft, imageBottom, imageWidth, imageHeight);

        //Debug.Log("=== 選取框在 Image 上的百分比位置 ===");
        //Debug.Log($"左上角: X={topLeftPercent.x:F2}%, Y={topLeftPercent.y:F2}%");
        //Debug.Log($"右上角: X={topRightPercent.x:F2}%, Y={topRightPercent.y:F2}%");
        //Debug.Log($"左下角: X={bottomLeftPercent.x:F2}%, Y={bottomLeftPercent.y:F2}%");
        //Debug.Log($"右下角: X={bottomRightPercent.x:F2}%, Y={bottomRightPercent.y:F2}%");

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

    // 計算單個點在 Image 上的百分比位置
    Vector2 CalculatePercentagePosition(Vector2 screenPoint, float imageLeft, float imageBottom, float imageWidth, float imageHeight)
    {
        // 計算相對於 Image 左下角的位置
        float relativeX = screenPoint.x - imageLeft;
        float relativeY = screenPoint.y - imageBottom;

        // 轉換為百分比 (0-100)
        float percentX = Mathf.Clamp01(relativeX / imageWidth) * 100;
        float percentY = Mathf.Clamp01(relativeY / imageHeight) * 100;

        return new Vector2(percentX, percentY);
    }

    // 公開方法：獲取選取框在指定 Image 上的百分比位置
    public Dictionary<string, Vector2> GetSelectionPercentageOnImage(RectTransform image)
    {
        if (image == null || !isDragging) return null;

        // 獲取 Image 的世界座標邊界
        Vector3[] imageCorners = new Vector3[4];
        image.GetWorldCorners(imageCorners);

        // 轉換為屏幕座標
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

    // 公開方法：檢查是否正在拖拽
    public bool IsDragging()
    {
        return isDragging;
    }

    // 公開方法：獲取選取框的矩形區域
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