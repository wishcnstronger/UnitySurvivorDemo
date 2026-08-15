# describe_image.py —— 截图「看图」工具
#
# 作用：把 Unity 截图发送给智谱 GLM-4V-Flash（支持视觉的模型），返回中文画面描述。
#       让没有视觉能力的 DeepSeek 也能「看到」游戏画面，再做分析。
#
# 用法：
#   py -3 Tools/describe_image.py [图片路径] [模型名]
#     - 图片路径省略时，自动取 Assets/Screenshots/ 下最新的一张截图
#     - 模型名默认 glm-4v-flash，也可指定 glm-4v-plus 等
#
# Key 配置（二选一）：
#   1. 环境变量 VISION_API_KEY
#   2. Tools/.vision_key 文件（一行纯文本，已加入 .gitignore，不会被提交）
import sys, os, json, base64, glob, urllib.request, urllib.error

# 强制 UTF-8 输出，避免 Windows 控制台 GBK 乱码
if hasattr(sys.stdout, "reconfigure"):
    sys.stdout.reconfigure(encoding="utf-8")
    sys.stderr.reconfigure(encoding="utf-8")

# 当前脚本所在目录（Tools/），用于定位密钥文件
TOOL_DIR = os.path.dirname(os.path.abspath(__file__))
KEY_FILE = os.path.join(TOOL_DIR, ".vision_key")
# 智谱 OpenAI 兼容接口地址
DEFAULT_URL = "https://open.bigmodel.cn/api/paas/v4/chat/completions"


def get_key():
    """优先读环境变量，其次读 .vision_key 文件。"""
    env = os.environ.get("VISION_API_KEY", "").strip()
    if env:
        return env
    if os.path.exists(KEY_FILE):
        with open(KEY_FILE, "r", encoding="utf-8") as f:
            return f.read().strip()
    return ""


def latest_screenshot():
    """取 Assets/Screenshots/ 下最新的 .png 截图。"""
    shots = sorted(glob.glob(r"Assets/Screenshots/*.png"), key=os.path.getmtime)
    return shots[-1] if shots else None


def main():
    img = sys.argv[1] if len(sys.argv) > 1 else latest_screenshot()
    model = sys.argv[2] if len(sys.argv) > 2 else "glm-4v-flash"
    key = get_key()
    base = os.environ.get("VISION_BASE_URL", DEFAULT_URL)

    if not img:
        print("[错误] 没找到截图。请先截图，确保 Assets/Screenshots/ 下有 .png 文件。")
        sys.exit(1)
    if not key:
        print("[错误] 没有 API Key。请二选一配置：")
        print("  方式一：setx VISION_API_KEY \"你的key\" （设置后重开终端）")
        print("  方式二：把 key 写入 Tools/.vision_key 文件（一行，不提交到 git）")
        sys.exit(1)

    # 图片转 base64，随请求发给视觉模型
    with open(img, "rb") as f:
        b64 = base64.b64encode(f.read()).decode()

    body = {
        "model": model,
        "messages": [{
            "role": "user",
            "content": [
                {"type": "image_url", "image_url": {"url": f"data:image/png;base64,{b64}"}},
                {"type": "text", "text": "请用中文详细描述这张图片：画面内容、布局、物体、颜色、文字、UI 元素，"
                                          "以及是否有异常或报错。只输出描述。"},
            ],
        }],
    }
    req = urllib.request.Request(
        base,
        data=json.dumps(body).encode(),
        headers={"Content-Type": "application/json", "Authorization": f"Bearer {key}"},
    )
    try:
        resp = json.load(urllib.request.urlopen(req, timeout=120))
        print(resp["choices"][0]["message"]["content"])
    except urllib.error.HTTPError as e:
        print(f"[错误] API 返回 {e.code}: {e.read().decode('utf-8', 'ignore')}")
        sys.exit(1)
    except Exception as e:
        print(f"[错误] 调用失败: {e}")
        sys.exit(1)


if __name__ == "__main__":
    main()
