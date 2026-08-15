"""通过 codely Unity MCP (Streamable HTTP) 调用工具的辅助脚本。
用法:
  py -3 Tools/mcp.py unity_editor '{"action":"get_current_state"}'
  py -3 Tools/mcp.py unity_screenshot '{"action":"capture_game_view","path":"Assets/Screenshots","filename":"check_combat"}'
输出 JSON 结果到 stdout。
"""
import json
import re
import sys
import urllib.request

sys.stdout.reconfigure(encoding="utf-8", errors="replace")

URL = "http://127.0.0.1:8080/mcp"
SID_FILE = "Tools/.mcp_session_id"


def get_session():
    try:
        with open(SID_FILE, "r") as f:
            return f.read().strip()
    except FileNotFoundError:
        return None


def set_session(sid):
    with open(SID_FILE, "w") as f:
        f.write(sid)


def post(payload, sid=None):
    headers = {
        "Content-Type": "application/json",
        "Accept": "application/json, text/event-stream",
    }
    if sid:
        headers["mcp-session-id"] = sid
    req = urllib.request.Request(
        URL, data=json.dumps(payload).encode(), headers=headers, method="POST"
    )
    with urllib.request.urlopen(req, timeout=30) as resp:
        body = resp.read().decode("utf-8")
        resp_sid = resp.headers.get("mcp-session-id")
    return body, resp_sid


def parse_result(body):
    """SSE 响应可能有多条 data: 行，取 id 匹配的 result，或最后一条 result。"""
    results = []
    for m in re.finditer(r"data: (\{.*\})\n?", body, re.DOTALL):
        try:
            results.append(json.loads(m.group(1)))
        except Exception:
            pass
    if not results:
        # 可能是纯 JSON（无 SSE 前缀）
        try:
            results.append(json.loads(body))
        except Exception:
            pass
    for r in results:
        if "result" in r:
            return r["result"]
        if "error" in r:
            return r["error"]
    return results[-1] if results else body


def main():
    tool = sys.argv[1]
    params = json.loads(sys.argv[2]) if len(sys.argv) > 2 else {}

    sid = get_session()
    if sid is None:
        body, sid = post({"jsonrpc": "2.0", "id": 1, "method": "initialize",
                          "params": {"protocolVersion": "2025-03-26", "capabilities": {},
                                     "clientInfo": {"name": "claude", "version": "1"}}})
        if sid:
            set_session(sid)
    body, _ = post({"jsonrpc": "2.0", "id": 2, "method": "tools/call",
                    "params": {"name": tool, "arguments": params}}, sid=sid)
    result = parse_result(body)
    print(json.dumps(result, ensure_ascii=False, indent=1))


if __name__ == "__main__":
    main()
