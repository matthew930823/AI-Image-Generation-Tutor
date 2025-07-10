using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class SelectImage : MonoBehaviour
{
    private string ConTrolNetPath = "ConTrolNet�Ѧҹ�/other"; // �۹� StreamingAssets
    public GameObject imageButtonPrefab; // �̭��t�� Button + RawImage
    public Transform imageGridContainer; // ��Ϥ��� Content
    public Image selectedImageDisplay; // ��ܳQ������Ϥ�
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
            Debug.LogError("���J���ѡG" + uwr.error);
            yield break;
        }

        Texture2D tex = UnityEngine.Networking.DownloadHandlerTexture.GetContent(uwr);
        Sprite sprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)  // pivot ����
            );
        buttonObj.GetComponentInChildren<Image>().sprite = sprite;

        // �I������ܦb�D���Ϥ�
        buttonObj.GetComponent<Button>().onClick.AddListener(() =>
        {
            Sprite newSprite = Sprite.Create(
                tex,
                new Rect(0, 0, tex.width, tex.height),
                new Vector2(0.5f, 0.5f)  // pivot ����
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
