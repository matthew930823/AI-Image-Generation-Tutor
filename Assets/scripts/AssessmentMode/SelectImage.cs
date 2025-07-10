using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class SelectImage : MonoBehaviour
{
    private string ConTrolNetPath = "ConTrolNet參考圖/other"; // 相對 StreamingAssets
    public GameObject imageButtonPrefab; // 裡面含有 Button + RawImage
    public Transform imageGridContainer; // 放圖片的 Content
    public Image selectedImageDisplay; // 顯示被選取的圖片
    public GameObject SelectPanel;
    // Start is called before the first frame update
    void Start()
    {
        string baseFolder = Path.Combine(Application.streamingAssetsPath, ConTrolNetPath);

        string[] imageFiles = Directory.GetFiles(baseFolder, "*.*")
            .Where(f => f.EndsWith(".png") || f.EndsWith(".jpg") || f.EndsWith(".jpeg"))
            .ToArray();

        foreach (string path in imageFiles)
        {
            GameObject newBtn = Instantiate(imageButtonPrefab, imageGridContainer);
            StartCoroutine(LoadImageToButton(path, newBtn));
        }
    }
    IEnumerator LoadImageToButton(string path, GameObject buttonObj)
    {
        string url = "file://" + path;
        UnityEngine.Networking.UnityWebRequest uwr = UnityEngine.Networking.UnityWebRequestTexture.GetTexture(url);
        yield return uwr.SendWebRequest();

        if (uwr.result != UnityEngine.Networking.UnityWebRequest.Result.Success)
        {
            Debug.LogError("載入失敗：" + uwr.error);
            yield break;
        }

        Texture2D tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
        Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)  // pivot 中心
            );
        buttonObj.GetComponentInChildren<Image>().sprite = sprite;

        // 點擊後顯示在主視圖中
        buttonObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            Sprite newSprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)  // pivot 中心
            );
            selectedImageDisplay.sprite = newSprite;
            SetPanelClose();
        });
    }
    public void SetPanelOpen()
    {
        SelectPanel.SetActive(true);
    }
    public void SetPanelClose()
    {
        SelectPanel.SetActive(false);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
