# AI-Image-Generation-Tutor
一個基於 Stable Diffusion 的互動式教學系統，幫助使用者學習 AI 圖像生成的原理與操作，同時附有「對話式圖像生成代理人」幫助使用者透過對話生成圖片。

## 安裝方式
請依照以下步驟完成準備工作：

1. 前往 [Releases](https://github.com/matthew930823/AI-Image-Generation-Tutor/releases) 下載 **AI-Image-Generation-Tutor** 並解壓縮
2. 前往 [Github](https://github.com/AUTOMATIC1111/stable-diffusion-webui) 下載 **stable-diffusion-webui-master** 並解壓縮
3. 利用記事本編輯 `SD/webui-user.bat` 在 `set COMMANDLINE_ARGS=` 後面加上 `--xformers --autolaunch --theme dark --api`
4. 開啟 `SD/webui-user.bat`，等待網頁 `http://127.0.0.1:7860` 跳出
5. 在 `cmd` 打上 `CTRL+C` + `Y` 結束執行
6. 前往 [Google Drive](https://drive.google.com/drive/folders/1Vom0m_9n0nxXxxJFo5NQgCTdfW7gIAMk?usp=sharing) 下載 **models** 資料夾後取代 `stable-diffusion-webui-master\models`
7. 前往 [Google Drive](https://drive.google.com/file/d/1aWy3xzgNLw_FiXXJ-svKDx5xsQhZA4On/view?usp=sharing) 下載 **extensions** 資料夾後取代 `stable-diffusion-webui-master\extensions`
8. 前往 [Google Drive](https://drive.google.com/file/d/14Ep12LbgJix1XQkQgH7gSwTmBTW5cKG2/view?usp=sharing) 下載 **embeddings** 資料夾後取代 `stable-diffusion-webui-master\embeddings`

## 使用方式
請依照以下步驟啟動系統：

1. 開啟 `SD/webui-user.bat`，等待網頁`http://127.0.0.1:7860`跳出  
2. 執行 `AI-Image-Generation-Tutor/AI圖像生成助教執行檔/AItest`
3. 若遇到問題，請嘗試重複步驟 1–2；也有可能是 LLM 被過度呼叫，請稍後再試  

# 系統操作範例
相關模式操作可參考 [影片說明](https://youtu.be/dvgTzGWRYK8) 

## LoRA 模型資料集
本專題自訓練之 LoRA 模型資料集可於以下連結下載：  
👉 [Google Drive 下載](https://drive.google.com/drive/folders/1KJ8zi5uhN3mLTzKApngOKMTSjnxnZAYD?usp=sharing)
