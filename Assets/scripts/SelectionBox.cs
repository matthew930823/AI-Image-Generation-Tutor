using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using System.Text.RegularExpressions;

public class SelectionBox : MonoBehaviour
{
    [Header("UI Elements")]
    public RectTransform selectionBoxUI;
    public Canvas canvas;
    public RectTransform targetImage; // 目標的 Image RectTransform

    private Camera uiCamera;

    private Vector2 startPosition;
    private Vector2 endPosition;
    private bool isDragging = false;

    public StableDiffusionRegionPrompt stableDiffusionRegionPrompt;
    List<string> result;
    List<string> B = new List<string>();
    // 選取框的四個角的座標
    public Vector2 TopLeft { get; private set; }
    public Vector2 TopRight { get; private set; }
    public Vector2 BottomLeft { get; private set; }
    public Vector2 BottomRight { get; private set; }

    void Start()
    {
        stableDiffusionRegionPrompt.gameController.voiceAudioPlayer.AudioPlay(0);
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
        // 開始拖曳
        if (Input.GetMouseButtonDown(0))
        {
            // 檢查是否點擊在 targetImage 內
            if (IsPointInTargetImage(Input.mousePosition))
            {
                StartSelection();
            }
        }

        // 結束拖曳
        if (Input.GetMouseButtonUp(0) && isDragging)
        {
            EndSelection();
        }
    }

    // 檢查點擊位置是否在 targetImage 內
    bool IsPointInTargetImage(Vector2 screenPoint)
    {
        if (targetImage == null) return false;

        // 將螢幕座標轉換為 targetImage 的本地座標
        Vector2 localPoint;
        bool isInside = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            targetImage,
            screenPoint,
            uiCamera,
            out localPoint
        );

        // 檢查點是否在 RectTransform 的範圍內
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

        // 限制拖曳範圍在 targetImage 內
        endPosition = ClampToTargetImage(endPosition);

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

        // 更新四個角的座標
        UpdateCornerPositions();
    }

    // 限制座標在 targetImage 範圍內
    Vector2 ClampToTargetImage(Vector2 screenPoint)
    {
        if (targetImage == null) return screenPoint;

        // 獲取 targetImage 的世界座標邊界
        Vector3[] imageCorners = new Vector3[4];
        targetImage.GetWorldCorners(imageCorners);

        // 轉換為螢幕座標
        Vector2 imageBottomLeft = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[0]);
        Vector2 imageTopRight = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[2]);

        // 限制在 Image 範圍內
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

        // 隱藏選取框
        if (selectionBoxUI != null)
            //selectionBoxUI.gameObject.SetActive(false);

            // 計算並輸出在 Image 上的百分比位置
            if (targetImage != null)
            {
                CalculateImagePercentagePositions();
            }
    }

    // 計算選取框四個角在 Image 上的百分比位置
    void CalculateImagePercentagePositions()
    {
        if (targetImage == null) return;

        // 獲取 Image 的世界座標邊界
        Vector3[] imageCorners = new Vector3[4];
        targetImage.GetWorldCorners(imageCorners);

        // 轉換為螢幕座標
        Vector2 imageBottomLeft = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[0]);
        Vector2 imageTopRight = RectTransformUtility.WorldToScreenPoint(uiCamera, imageCorners[2]);

        // 計算 Image 的螢幕座標範圍
        float imageLeft = imageBottomLeft.x;
        float imageRight = imageTopRight.x;
        float imageBottom = imageBottomLeft.y;
        float imageTop = imageTopRight.y;

        float imageWidth = imageRight - imageLeft;
        float imageHeight = imageTop - imageBottom;

        // 計算各角的百分比位置
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

        // 轉換為螢幕座標
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

    // 公開方法：檢查是否正在拖曳
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

    public void CheckAnswerForSelect(InputField inputField)
    {
        string LLMPrompt= $@"
接下來我會給你三串文字，分別為1.a 2.b，其中b會由多個中括號([])包住的詞組成，否則如果a的詞和b之中的某個詞意思相近，如果a的詞和b之中的某個詞意思相近，你需要回傳「回答正確」並把b中意思相近的詞也回傳出來，如果a沒有和b的任何詞意思相近，你需要回傳「回答錯誤」，判斷標準不用太嚴格，只要主體一樣就算對，就算是不同的語言只要意思相近就行，需要說明原因

a:{inputField.text}

b:{string.Join(", ", result.Select(x => $"[{x}]"))}

輸出為: 結果:{{回答正確/回答錯誤}} 意思相近的字:{{(若結果不是回答正確則留白)}} 請將結果和意思相近的字務必都放進{{}}裡面
";
        Debug.Log(LLMPrompt);
        StartCoroutine(stableDiffusionRegionPrompt.geminiAPI.SendRequest(LLMPrompt,(r)=> {
            MatchCollection matches = Regex.Matches(r, @"\{(.*?)\}");
            List<string> results = new List<string>();

            foreach (Match match in matches)
            {
                results.Add(match.Groups[1].Value); // 只抓 {} 中的內容，不含大括號
            }
            if(results[0]== "回答正確")
            {
                results[1] = results[1].Replace("[", "").Replace("]", "");
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
}