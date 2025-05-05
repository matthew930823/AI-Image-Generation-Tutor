using UnityEngine;
using UnityEngine.UI;

public class SelectionBoxController : MonoBehaviour
{
    public RectTransform selectionBox;
    private Vector2 startPos;
    private Vector2 endPos;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            // �}�l�즲
            startPos = Input.mousePosition;
            Debug.Log(startPos);
            selectionBox.gameObject.SetActive(true);
        }
        else if (Input.GetMouseButton(0))
        {
            // �즲��
            endPos = Input.mousePosition;
            UpdateSelectionBox();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            // �즲����
            selectionBox.gameObject.SetActive(false);
            // �i�H�b�o��Ĳ�o����d�򪺧P�_
        }
    }

    void UpdateSelectionBox()
    {
        Vector2 boxStart = startPos;
        Vector2 boxSize = endPos - startPos;
        int screenWidth = Screen.width;
        int screenHeight = Screen.height;
        if (boxSize.x < 0)
        {
            boxStart.x = endPos.x;
            boxSize.x = -boxSize.x;
        }
        if (boxSize.y < 0)
        {
            boxStart.y = endPos.y;
            boxSize.y = -boxSize.y;
        }
        boxStart.x -= screenWidth / 2;
        boxStart.y -= screenHeight / 2;
        selectionBox.anchoredPosition = boxStart;
        selectionBox.sizeDelta = boxSize;
    }
}
