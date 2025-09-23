# AI-Image-Generation-Tutor
一個基於 Stable Diffusion 的互動式教學系統，幫助使用者學習 AI 圖像生成的原理與操作，同時附有「對話式圖像生成代理人」幫助使用者透過對話生成圖片。

## 使用方式
請依照以下步驟啟動系統：

1. 前往 [Releases](https://github.com/matthew930823/AI-Image-Generation-Tutor/releases) 下載 **AI-Image-Generation-Tutor** 並解壓縮
2. 前往 [Google Drive](https://drive.google.com/drive/folders/1KJ8zi5uhN3mLTzKApngOKMTSjnxnZAYD?usp=sharing) 下載 **SD** 資料夾 > ⚠️ 注意：下載可能需要較長時間
3. 前往 `SD\venv\Lib\site-packages\jax\_src` 將 `custom_derivatives.zip` 解壓縮，並把解壓出的 `custom_derivatives.py` 複製到 `SD\venv\Lib\site-packages\jax\_src\` 資料夾
4. 開啟 `SD/webui-user.bat`，等待網頁`http://127.0.0.1:7860`跳出  
5. 執行 `AI-Image-Generation-Tutor/AI圖像生成助教執行檔/AItest`
6. 若遇到問題，請嘗試重複步驟 4–5；也有可能是 LLM 被過度呼叫，請稍後再試  

# 系統操作範例
相關模式操作可參考 [影片說明](https://youtu.be/dvgTzGWRYK8) 

## LoRA 模型資料集
本專題自訓練之 LoRA 模型資料集可於以下連結下載：  
👉 [Google Drive 下載](https://drive.google.com/drive/folders/1KJ8zi5uhN3mLTzKApngOKMTSjnxnZAYD?usp=sharing)
