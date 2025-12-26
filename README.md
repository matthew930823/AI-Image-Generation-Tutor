# AI Image Generation Tutor: Guiding Learning with a Conversational Agent
# AI 圖像生成助教：建構一個對話式圖像生成代理人並引導學習

## 📖 專案簡介 (Introduction)

本專題開發了一套互動式圖像生成教學系統，旨在引導使用者從基礎的提示詞輸入，進階至能夠精確操控圖像生成的專家。系統結合了大型語言模型（LLM）、虛擬角色與語音回饋，提供多樣化的學習模式與考核機制。

此外，系統包含一個**對話式圖像生成代理人**，讓使用者能透過自然語言對話，經由系統自動提取參數並運用 ControlNet、LoRA 等技術，生成符合需求的理想圖像。

## 主要功能 (Key Features)

本系統設計了五種互動模式，滿足不同階段的學習需求：

### 1. 簡單模式 (Easy Mode)
- **目的**：了解單一控制技術的效果。
- **內容**：透過圖像對照，學習模型選擇、提示詞（Prompt）、LoRA、解析度與 ControlNet 對生成結果的影響。

### 2. 困難模式 (Hard Mode)
- **目的**：了解多種控制技術混合使用的效果。
- **內容**：觀察一張融合多種控制技術的圖片，判斷其使用了哪些具體的技術組合。

### 3. 猜詞模式 (Guess Prompt Mode)
- **目的**：強化對提示詞的撰寫能力。
- **內容**：運用 MultiDiffusion 技術生成多區域控制圖像，使用者需觀察並推測特定區域的提示詞。

### 4. 考核模式 (Exam Mode)
- **目的**：評量對控制技術的掌握程度。
- **內容**：使用者需自行設定參數，嘗試生成與題目極為相似的圖像，系統將量化相似度進行評分。

### 5. 匠人模式 (Craftsman Mode)
- **目的**：整合所有技術，協助自由創作。
- **特色**：
    - **對話式代理人 (Conversational Agent)**：透過與虛擬助教對話，系統自動將需求轉化為專業的生成參數。
    - 提供逐步引導介面，讓使用者自定義風格、姿勢、構圖等細節。

## 🛠️ 技術架構 (Technology Stack)

本系統整合了多項前沿 AI 技術與開發工具：

- **核心引擎**: Unity
- **大型語言模型 (LLM)**: Gemini 2.5 Flash-Lite (負責題目生成、資訊提取、對話互動)
- **圖像生成模型**: Stable Diffusion v1.5
- **語音生成**: CozyVoice
- **虛擬人物**: Live2D
- **生成控制與微調技術**:
  - **LoRA**: 用於特定風格與角色的微調 (包含自訓練模型)。
  - **ControlNet**: 用於精確控制構圖與姿勢 (OpenPose, Depth, Canny, Shuffle)。
  - **Textual Inversion**: 優化生成品質，避免負面特徵。
  - **ADetailer**: 自動修復臉部與手部細節。
  - **MultiDiffusion**: 支援區域控制生成。

## 🎥 成果展示 (Demo)

觀看系統實際操作與功能展示影片：
[點擊觀看 YouTube 影片](https://youtu.be/dvgTzGWRYK8)

## 👥 作者與貢獻 (Authors)

**國立臺灣海洋大學 資訊工程學系 (NTOU CS)**
指導教授：張欽圳 博士

| 姓名 | 學號 | 主要職責 | 貢獻度 |
| :--- | :--- | :--- | :--- |
| **楊宏立** | 01157025 | 系統設計、技術學習、LoRA模型自訓練、實驗設計與測試 | 50% |
| **李孟修** | 01157005 | 系統實作、語音生成整合、虛擬人物整合、報告撰寫 | 50% |

## 📄 參考文獻 (References)

本專案參考了 Stable Diffusion, ControlNet, LoRA, MultiDiffusion 等多項學術研究與開源技術，詳細列表請參閱專題報告。

## 安裝方式
請依照以下步驟完成準備工作：

1. 前往 [Releases](https://github.com/matthew930823/AI-Image-Generation-Tutor/releases) 下載 **AI-Image-Generation-Tutor** 並解壓縮
2. 前往 [Github](https://github.com/AUTOMATIC1111/stable-diffusion-webui) 下載 **stable-diffusion-webui-master** 並解壓縮
3. 利用記事本編輯 `SD/webui-user.bat` 在 `set COMMANDLINE_ARGS=` 後面加上 `--xformers --autolaunch --theme dark --api`
4. 開啟 `SD/webui-user.bat`，等待網頁 `http://127.0.0.1:7860` 跳出
5. 在 `cmd` 打上 `CTRL+C` + `Y` 結束執行
6. 前往 [Google Drive](https://drive.google.com/file/d/1225faesdEkALrjyPjGiuyLtPkeUhbBYR/view?usp=sharing) 下載 **models** 資料夾後取代 `stable-diffusion-webui-master\models`
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

---
*Created as part of the Graduation Project at National Taiwan Ocean University, 2025.*
