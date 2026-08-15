@echo off
REM 看图工具快捷入口：截图 -> 视觉模型 -> 中文描述
REM 用法: describe_image [图片路径] [模型名]
REM 图片路径省略时自动取 Assets/Screenshots/ 下最新截图
chcp 65001 >nul
cd /d "%~dp0.."
py -3 "Tools\describe_image.py" %*
